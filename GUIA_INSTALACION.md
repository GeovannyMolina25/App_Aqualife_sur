# 🚀 GUÍA DE INSTALACIÓN — ROTTER Sistema de Ventas de Agua

## 1. PROGRAMAS A INSTALAR (en este orden)

| Programa           | Versión    | Descarga                                                   |
|--------------------|------------|------------------------------------------------------------|
| SQL Server 2019    | Developer  | https://www.microsoft.com/sql-server/sql-server-downloads  |
| SSMS               | Última     | https://aka.ms/ssmsfullsetup                               |
| .NET 8 SDK         | 8.x        | https://dotnet.microsoft.com/download/dotnet/8.0           |
| Node.js            | LTS v20+   | https://nodejs.org/                                        |
| Angular CLI v19    | —          | `npm install -g @angular/cli@19`                           |

---

## 2. BASE DE DATOS

1. Abre **SSMS** → conecta con usuario `sa` y tu contraseña
2. Archivo → Abrir → `sql/01_crear_base_datos.sql`
3. Presiona **F5**
4. Verifica el mensaje: `✅ RotterDB creada exitosamente.`

---

## 3. BACKEND

### 3.1 Configura la contraseña de SQL Server
Abre `backend/rotter.API/appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Database=RotterDB;User Id=sa;Password=TU_CONTRASEÑA;TrustServerCertificate=True;"
```

### 3.2 Ejecuta estos comandos (en orden):
```bash
cd backend/rotter.API

# Instalar herramienta EF (solo primera vez)
dotnet tool install --global dotnet-ef

# Restaurar paquetes NuGet
dotnet restore

# Crear migración inicial
dotnet ef migrations add InitialCreate --project ../rotter.Infraestructura

# Aplicar migración a la BD
dotnet ef database update

# Iniciar el servidor
dotnet run
```

**Verificación:** Abre http://localhost:5000/swagger — verás los 5 controllers.

---

## 4. FRONTEND

Abre una **nueva terminal** (el backend debe seguir corriendo):

```bash
cd frontend

# Instalar dependencias (2-5 minutos)
npm install

# Copiar tu logo
# → Pon tu imagen en: frontend/src/assets/logo.png

# Iniciar la aplicación
ng serve
```

**Verificación:** Abre http://localhost:4200 — verás la pantalla de login.

---

## 5. CREAR PRIMER ADMINISTRADOR

```bash
# 1. Ve a http://localhost:4200/auth/registro
# 2. Crea tu cuenta con email y contraseña
# 3. En SSMS ejecuta:
UPDATE Usuarios SET RolId = 1 WHERE Email = 'tu@email.com';
# 4. Inicia sesión → eres Administrador
```

---

## 6. ESTRUCTURA DEL PROYECTO

```
rotter/
├── backend/
│   ├── Rotter.sln
│   ├── rotter.Dominio/
│   │   ├── Entidades/          ← Rol.cs | Usuario.cs | Producto.cs | Venta.cs | DetalleVenta.cs | Auditoria.cs
│   │   ├── DTOs/
│   │   │   ├── Auth/           ← LoginDto.cs | RegistroDto.cs | AuthResponseDto.cs
│   │   │   ├── Comun/          ← RespuestaDto.cs | PagedResult.cs
│   │   │   ├── Usuarios/       ← UsuarioDto.cs | CambiarRolDto.cs
│   │   │   ├── Productos/      ← ProductoDto.cs | CrearProductoDto.cs
│   │   │   └── Ventas/         ← VentaDto.cs | DetalleVentaDto.cs | CrearVentaDto.cs | MetricasDto.cs
│   │   └── Interfaces/
│   │       ├── Repositorios/   ← IUsuarioRepositorio.cs | IProductoRepositorio.cs | IVentaRepositorio.cs | IAuditoriaRepositorio.cs
│   │       └── Servicios/      ← IJwtServicio.cs | IAuditoriaServicio.cs | IReporteServicio.cs
│   │
│   ├── rotter.Aplicacion/
│   │   ├── Auth/Commands/      ← LoginCommand.cs | RegistrarUsuarioCommand.cs
│   │   ├── Usuarios/Commands/  ← CambiarRolCommand.cs
│   │   ├── Usuarios/Queries/   ← ObtenerUsuariosQuery.cs
│   │   ├── Productos/Commands/ ← CrearProductoCommand.cs
│   │   ├── Productos/Queries/  ← ObtenerProductosQuery.cs | ObtenerPromocionesQuery.cs
│   │   ├── Ventas/Commands/    ← RegistrarVentaCommand.cs
│   │   ├── Ventas/Queries/     ← ObtenerVentasQuery.cs | ObtenerMetricasQuery.cs | ObtenerHistorialClienteQuery.cs
│   │   └── Reportes/Queries/   ← GenerarPdfMensualQuery.cs | GenerarPdfColaboradorQuery.cs | GenerarExcelQuery.cs
│   │
│   ├── rotter.Infraestructura/
│   │   ├── Data/               ← RotterDbContext.cs
│   │   ├── Repositorios/
│   │   │   ├── Usuarios/       ← UsuarioRepositorio.cs
│   │   │   ├── Productos/      ← ProductoRepositorio.cs
│   │   │   ├── Ventas/         ← VentaRepositorio.cs
│   │   │   └── Auditoria/      ← AuditoriaRepositorio.cs
│   │   └── Servicios/
│   │       ├── Auth/           ← JwtServicio.cs
│   │       ├── Auditoria/      ← AuditoriaServicio.cs
│   │       └── Reportes/       ← ReporteServicio.cs
│   │
│   └── rotter.API/
│       ├── Controllers/
│       │   ├── Auth/           ← AuthController.cs
│       │   ├── Usuarios/       ← UsuariosController.cs
│       │   ├── Productos/      ← ProductosController.cs
│       │   ├── Ventas/         ← VentasController.cs
│       │   └── Reportes/       ← ReportesController.cs
│       ├── Extensions/         ← ServiceExtensions.cs
│       ├── Middleware/         ← ExcepcionMiddleware.cs
│       ├── Program.cs
│       └── appsettings.json
│
├── frontend/
│   └── src/app/
│       ├── core/
│       │   ├── guards/         ← auth.guard.ts | admin.guard.ts | colaborador.guard.ts
│       │   ├── interceptors/   ← jwt.interceptor.ts
│       │   ├── models/
│       │   │   ├── auth/       ← login.model.ts | registro.model.ts | auth-response.model.ts
│       │   │   ├── comun/      ← respuesta.model.ts
│       │   │   ├── usuarios/   ← usuario.model.ts
│       │   │   ├── productos/  ← producto.model.ts
│       │   │   └── ventas/     ← venta.model.ts | metricas.model.ts
│       │   └── services/
│       │       ├── auth/       ← auth.service.ts
│       │       ├── usuarios/   ← usuarios.service.ts
│       │       ├── productos/  ← productos.service.ts
│       │       ├── ventas/     ← ventas.service.ts
│       │       └── reportes/   ← reportes.service.ts
│       ├── features/
│       │   ├── auth/login/     ← login.component.ts | .html | .css
│       │   ├── auth/registro/  ← registro.component.ts | .html | .css
│       │   ├── layout/         ← layout.component.ts | .html | .css
│       │   ├── dashboard/      ← dashboard.component.ts | .html | .css
│       │   ├── productos/      ← productos.component.ts | .html | .css
│       │   ├── ventas/         ← ventas.component.ts | .html | .css
│       │   ├── historial/      ← historial.component.ts | .html | .css
│       │   ├── usuarios/       ← usuarios.component.ts | .html | .css
│       │   └── reportes/       ← reportes.component.ts | .html | .css
│       └── shared/components/
│           ├── spinner/        ← spinner.component.ts
│           ├── alert/          ← alert.component.ts
│           ├── badge/          ← badge.component.ts
│           ├── pagination/     ← pagination.component.ts
│           └── modal/          ← modal.component.ts
│
├── sql/
│   └── 01_crear_base_datos.sql
└── docs/
    └── GUIA_INSTALACION.md
```

---

## 7. ROLES Y PERMISOS

| Función               | Admin | Colaborador | Cliente |
|-----------------------|:-----:|:-----------:|:-------:|
| Ver productos         |  ✓    |     ✓       |   ✓     |
| Ver promociones       |  ✓    |     ✓       |   ✓     |
| Crear productos       |  ✓    |     ✓       |   ✗     |
| Registrar ventas      |  ✓    |     ✓       |   ✓*    |
| Ver todas las ventas  |  ✓    |     ✓       |   ✗     |
| Ver mi historial      |  ✓    |     ✓       |   ✓     |
| Ver métricas          |  ✓    |     ✗       |   ✗     |
| Gestionar usuarios    |  ✓    |     ✗       |   ✗     |
| Cambiar roles         |  ✓    |     ✗       |   ✗     |
| Generar reportes PDF  |  ✓    |     ✗       |   ✗     |
| Generar Excel         |  ✓    |     ✗       |   ✗     |

---

## 8. SOLUCIÓN DE PROBLEMAS

**Error: "Cannot connect to SQL Server"**
→ Abre Servicios de Windows y verifica que "SQL Server (MSSQLSERVER)" esté iniciado.

**Error: "dotnet command not found"**
→ Reinstala .NET 8 SDK y reinicia la terminal.

**Error: "ng command not found"**
→ `npm install -g @angular/cli@19` y reinicia la terminal.

**Error CORS en el navegador**
→ Verifica que el backend corra en puerto 5000. El frontend debe iniciarse DESPUÉS del backend.

**Error: migrations failed**
→ Verifica la cadena de conexión en `appsettings.json`. La contraseña debe ser exacta.

---

## 9. DESPLIEGUE EN PRODUCCIÓN

```bash
# Backend
cd backend/rotter.API
dotnet publish -c Release -o ./publish
# Subir /publish a IIS o servidor Linux

# Frontend
cd frontend
ng build --configuration production
# Subir dist/rotter-frontend/ a Nginx / Apache / IIS
```
