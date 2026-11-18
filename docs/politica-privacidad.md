# Política de Privacidad y Tratamiento de Datos Personales — [RAZÓN SOCIAL]

> **⚠️ ESTO ES UN BORRADOR, NO UN DOCUMENTO LEGAL VÁLIDO TODAVÍA.**
> Fue redactado como punto de partida alineado a la Ley Orgánica de Protección de Datos
> Personales de Ecuador (LOPDP, Registro Oficial Suplemento 459 del 26 de mayo de 2021) y su
> Reglamento General. **Debe ser revisado y aprobado por un abogado ecuatoriano especializado
> en protección de datos antes de publicarlo**, quien debe verificar:
> - Los datos legales de la empresa (marcados como `[PLACEHOLDER]` abajo).
> - El estado de implementación vigente de la LOPDP y de la Superintendencia de Protección de
>   Datos Personales (la ley tiene un régimen de transición progresivo; confirma qué obligaciones
>   ya son exigibles a la fecha en que publiques esto).
> - Si la empresa está obligada a designar un Delegado de Protección de Datos (DPO) según el
>   volumen y tipo de datos que trata.
> - Cualquier requisito sectorial adicional (ej. si en el futuro se procesan pagos con tarjeta,
>   aplicarían además obligaciones de PCI-DSS que este documento no cubre).

**Última actualización:** [FECHA]

## 1. Responsable del tratamiento

| Campo | Dato |
|---|---|
| Razón social | [RAZÓN SOCIAL / NOMBRE COMERCIAL] |
| RUC | [RUC] |
| Dirección | [DIRECCIÓN FISCAL] |
| Representante legal | [NOMBRE DEL REPRESENTANTE LEGAL] |
| Correo de contacto para temas de datos personales | [EMAIL DE CONTACTO] |

## 2. Alcance

Esta política aplica a los datos personales que se recopilan a través de la aplicación web y
API de [NOMBRE DEL SISTEMA / APP] (en adelante, "la Plataforma"), utilizada para la gestión de
ventas, productos, clientes y pedidos de [RAZÓN SOCIAL].

## 3. Datos que recopilamos

Según la información que efectivamente procesa la Plataforma:

**Datos de cuenta (clientes, colaboradores y administradores):**
- Nombre y apellido
- Correo electrónico
- Contraseña (nunca se almacena en texto plano; se guarda únicamente su hash con bcrypt)
- Fecha de nacimiento
- Sexo
- Dirección
- Teléfono (opcional)
- Rol dentro del sistema (Cliente, Colaborador, Administrador, SuperAdministrador)

**Datos de pedidos y ventas:**
- Historial de compras y detalle de productos adquiridos
- Dirección de envío, teléfono de contacto y nombre del receptor del pedido
- Método de pago elegido (transferencia bancaria o pago contra entrega) — **no se recopilan
  ni almacenan datos de tarjetas de crédito/débito**
- Comprobantes de pago (imagen/PDF) subidos por el cliente para verificar transferencias

**Datos técnicos y de seguridad:**
- Dirección IP y user-agent del navegador, asociados a inicios de sesión y acciones relevantes
  (registro de auditoría de seguridad)
- Fecha y hora de accesos, cambios de contraseña y otros eventos de la cuenta

**Dato especial — verificación en dos pasos (2FA):** si el usuario activa la verificación en
dos pasos, se almacena de forma cifrada (AES-256) el secreto necesario para validar los códigos
de su aplicación autenticadora. Este secreto nunca se muestra ni se envía en texto plano después
de la activación inicial.

## 4. Finalidad del tratamiento

Los datos personales se tratan para:

1. Crear y administrar la cuenta del usuario en la Plataforma.
2. Procesar y dar seguimiento a pedidos y ventas.
3. Verificar pagos por transferencia bancaria.
4. Enviar comunicaciones operativas (confirmación de pedido, recuperación de contraseña, alertas
   de seguridad de la cuenta).
5. Prevenir fraude y accesos no autorizados (registro de auditoría, límite de intentos de
   inicio de sesión, verificación en dos pasos).
6. Cumplir obligaciones legales y tributarias aplicables a la actividad comercial de
   [RAZÓN SOCIAL].

No utilizamos los datos para perfilamiento publicitario ni los vendemos a terceros.

## 5. Base legal / consentimiento

El tratamiento se basa en:
- La ejecución de la relación contractual entre el usuario y [RAZÓN SOCIAL] al registrarse y
  realizar compras en la Plataforma.
- El consentimiento del titular, otorgado al aceptar esta política durante el registro.
- El cumplimiento de obligaciones legales (facturación, contabilidad).

El titular puede revocar su consentimiento en cualquier momento, sin que esto afecte la licitud
del tratamiento previo a la revocatoria, y sin perjuicio de que ciertos datos deban conservarse
por obligación legal (ej. facturación).

## 6. Encargados de tratamiento y transferencias

- **Envío de correos electrónicos:** utilizamos el servicio SMTP de Gmail (Google LLC) para
  enviar notificaciones a los usuarios (recuperación de contraseña, alertas de seguridad,
  confirmaciones). Esto puede implicar una transferencia internacional de datos hacia servidores
  de Google. [VERIFICAR CON EL ABOGADO SI SE REQUIERE UNA CLÁUSULA ESPECÍFICA DE TRANSFERENCIA
  INTERNACIONAL Y SI GOOGLE OFRECE GARANTÍAS ADECUADAS PARA ESTE CASO DE USO.]
- No compartimos datos personales con terceros para fines comerciales o publicitarios.
- [AGREGAR AQUÍ cualquier otro proveedor que procese datos en tu nombre: hosting, pasarela de
  pago si se integra en el futuro, servicio de mensajería para envíos, etc.]

## 7. Conservación de los datos

Los datos de la cuenta se conservan mientras el usuario mantenga una cuenta activa en la
Plataforma. Los datos de ventas y facturación se conservan durante el plazo que exige la
normativa tributaria y contable ecuatoriana vigente. Al eliminarse una cuenta, se conservarán
únicamente los datos que la ley exija retener (ej. comprobantes de venta), y se eliminará o
anonimizará el resto.

## 8. Medidas de seguridad implementadas

De forma consistente con el principio de responsabilidad proactiva de la LOPDP, la Plataforma
implementa, entre otras:

- Contraseñas hasheadas con bcrypt (nunca en texto plano).
- Autenticación mediante tokens JWT de corta duración, con renovación segura y revocación al
  cerrar sesión o cambiar la contraseña.
- Verificación en dos pasos (2FA) opcional, con el secreto cifrado en reposo.
- Límite de intentos de inicio de sesión y bloqueo temporal de cuenta ante intentos sospechosos.
- Validación de todas las entradas del usuario en el servidor.
- Cifrado del tráfico entre la aplicación y la base de datos, y HTTPS obligatorio en producción.
- Cabeceras de seguridad HTTP (CSP, X-Frame-Options, HSTS, entre otras).
- Registro de auditoría de accesos y acciones relevantes, sin almacenar contraseñas ni tokens
  en los registros.
- Alertas automáticas al titular ante bloqueos de cuenta o cambios en su configuración de 2FA.

## 9. Derechos del titular de los datos (Derechos ARCO+)

De acuerdo con la LOPDP, como titular de tus datos personales tienes derecho a:

- **Acceso:** conocer qué datos tuyos tratamos.
- **Rectificación y actualización:** corregir datos inexactos o desactualizados.
- **Eliminación / Cancelación:** solicitar la eliminación de tus datos, salvo obligación legal
  de conservarlos.
- **Oposición:** oponerte a un tratamiento específico de tus datos.
- **Portabilidad:** solicitar tus datos en un formato estructurado y de uso común.
- **Revocatoria del consentimiento:** en cualquier momento.
- **No ser objeto de decisiones automatizadas** que produzcan efectos jurídicos sobre ti sin
  intervención humana.

Para ejercer estos derechos, escribe a **[EMAIL DE CONTACTO]** indicando tu solicitud. Responderemos
dentro del plazo que establece la normativa vigente.

Si consideras que tus derechos no han sido atendidos adecuadamente, puedes presentar un reclamo
ante la **Superintendencia de Protección de Datos Personales** [VERIFICAR CANAL Y ESTADO ACTUAL
DE FUNCIONAMIENTO DE ESTA AUTORIDAD AL MOMENTO DE PUBLICAR ESTE DOCUMENTO].

## 10. Menores de edad

La Plataforma solicita la fecha de nacimiento al registrarse. [DEFINIR CON EL ABOGADO LA POLÍTICA
SOBRE MENORES DE EDAD: si el servicio está dirigido solo a mayores de edad, si se requiere
consentimiento de un representante legal para menores, y qué edad mínima se exige.]

## 11. Cambios a esta política

Podemos actualizar esta política para reflejar cambios en la Plataforma o en la normativa
aplicable. Notificaremos cambios sustanciales a través de la Plataforma o por correo electrónico.

## 12. Contacto

Para consultas sobre esta política o el tratamiento de tus datos personales, contáctanos en
**[EMAIL DE CONTACTO]**.
