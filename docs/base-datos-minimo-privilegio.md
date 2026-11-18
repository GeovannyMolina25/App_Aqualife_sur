# Mínimo privilegio en la base de datos

## El problema actual

`backend/rotter.API/appsettings.json` usa:

```
Server=localhost\SQLEXPRESS;Database=RotterDB_dev;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;
```

`Trusted_Connection=True` significa que la app se conecta con la cuenta de Windows que ejecuta
el proceso (en tu máquina de desarrollo, tu propio usuario). En SQL Server Express, esa cuenta
casi siempre termina siendo `sysadmin` porque es la que instaló/administra la instancia — es
decir, **la aplicación tiene permisos totales sobre todo el servidor SQL**, no solo sobre
`RotterDB_dev`, cuando en realidad solo necesita leer y escribir en las tablas de esa base.

Esto es aceptable para desarrollo local, pero **no debe replicarse en producción**: si alguien
compromete la aplicación (por ejemplo, mediante una vulnerabilidad futura no detectada), hereda
los mismos permisos que el proceso de la app tenga contra SQL Server. Cuanto menor sea ese
permiso, menor el daño posible.

## Recomendación para producción

1. Crear un **login SQL dedicado** para la aplicación (autenticación SQL, no Windows), con una
   contraseña fuerte generada aleatoriamente y guardada como secreto (variable de entorno o
   gestor de secretos), igual que ya se hace con `JWT:SecretKey`.
2. Ese login solo debe tener los roles `db_datareader` y `db_datawriter` **sobre `RotterDB_dev`**
   — nada a nivel de servidor, y nada de permisos DDL (crear/alterar/eliminar tablas).
3. **Importante:** `Program.cs` hoy ejecuta `Database.MigrateAsync()` automáticamente al arrancar
   la app (línea ~50-54). Esto requiere permisos DDL que un login de solo lectura/escritura no
   tiene. Con un login de mínimo privilegio, tienes dos opciones:
   - **Recomendada:** separar el paso de migración del arranque normal. Ejecutar las migraciones
     como un paso de despliegue aparte (`dotnet ef database update`, con un login que sí tenga
     permisos DDL, usado solo en ese momento), y que el proceso de la app en producción arranque
     sin `MigrateAsync()`, usando el login restringido.
   - **Alternativa más simple (menos aislada):** mantener `MigrateAsync()` en el arranque pero
     usar un login con el rol `db_ddladmin` además de `db_datareader`/`db_datawriter` — sigue
     siendo mucho más restringido que `sysadmin`, aunque no separa completamente el privilegio
     de "ejecutar la app" del privilegio de "cambiar el esquema".

## Script de ejemplo

Ver [`sql/04_crear_login_minimo_privilegio.sql`](../sql/04_crear_login_minimo_privilegio.sql).
Es idempotente (se puede ejecutar varias veces sin duplicar nada) y **no modifica datos
existentes**. Ejecútalo en SSMS contra el servidor de producción cuando lo tengas definido —
ajusta el nombre de la base de datos y la contraseña antes de correrlo.

Después de crear el login, actualiza la cadena de conexión en producción (vía variable de
entorno `ConnectionStrings__DefaultConnection`, no en `appsettings.json`) a algo como:

```
Server=tu-servidor;Database=RotterDB;User Id=rotter_app;Password=<desde el secreto>;Encrypt=True;TrustServerCertificate=True;
```

## Verificación

Tras aplicar el script, confirma que el login **no puede**:
- Crear ni eliminar tablas (`CREATE TABLE` / `DROP TABLE` deben fallar).
- Ver ni administrar otras bases de datos del servidor.
- Otorgar permisos a otros usuarios.

Y que **sí puede**:
- Hacer `SELECT`, `INSERT`, `UPDATE`, `DELETE` sobre las tablas de `RotterDB`.
