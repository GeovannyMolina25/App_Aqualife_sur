-- ================================================================
-- ROTTER — Base de datos SQL Server 2019
-- Ejecutar en SSMS o con: sqlcmd -S localhost -U sa -P "Pass" -i 01_crear_base_datos.sql
-- ================================================================

USE master;
GO

IF DB_ID('RotterDB') IS NOT NULL
BEGIN
    ALTER DATABASE RotterDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE RotterDB;
END
GO

CREATE DATABASE RotterDB COLLATE Latin1_General_CI_AS;
GO
USE RotterDB;
GO

-- ── Roles ─────────────────────────────────────────────────
CREATE TABLE Roles (
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(50)  NOT NULL UNIQUE,
    Descripcion   NVARCHAR(200) NULL,
    Activo        BIT           NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
INSERT INTO Roles (Nombre, Descripcion) VALUES
    ('Administrador', 'Acceso total al sistema'),
    ('Colaborador',   'Puede registrar ventas y crear productos'),
    ('Cliente',       'Puede ver productos e historial de compras');
GO

-- ── Usuarios ──────────────────────────────────────────────
CREATE TABLE Usuarios (
    Id                INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre            NVARCHAR(100) NOT NULL,
    Apellido          NVARCHAR(100) NOT NULL,
    Email             NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash      NVARCHAR(500) NOT NULL,
    FechaNacimiento   DATE          NOT NULL,
    Sexo              NVARCHAR(20)  NOT NULL CHECK (Sexo IN ('Masculino','Femenino','Otro')),
    Direccion         NVARCHAR(300) NOT NULL,
    Telefono          NVARCHAR(20)  NULL,
    RolId             INT           NOT NULL DEFAULT 3,
    Activo            BIT           NOT NULL DEFAULT 1,
    FechaCreacion     DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion DATETIME2     NULL,
    UltimoAcceso      DATETIME2     NULL,
    CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolId) REFERENCES Roles(Id)
);
CREATE INDEX IX_Usuarios_Email ON Usuarios(Email);
CREATE INDEX IX_Usuarios_RolId  ON Usuarios(RolId);
GO

-- ── Categorias ────────────────────────────────────────────
CREATE TABLE Categorias (
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Descripcion   NVARCHAR(300) NULL,
    Activo        BIT           NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2     NOT NULL DEFAULT GETUTCDATE()
);
INSERT INTO Categorias (Nombre, Descripcion) VALUES
    ('Agua Natural',   'Agua natural sin gas'),
    ('Agua con Gas',   'Agua carbonatada'),
    ('Agua Purificada','Proceso de purificación avanzado'),
    ('Bidón',          'Bidones para hogar y oficina'),
    ('Promociones',    'Paquetes y combos especiales');
GO

-- ── Productos ─────────────────────────────────────────────
CREATE TABLE Productos (
    Id                   INT             IDENTITY(1,1) PRIMARY KEY,
    Nombre               NVARCHAR(200)   NOT NULL,
    Descripcion          NVARCHAR(500)   NULL,
    Caracteristicas      NVARCHAR(1000)  NULL,
    Precio               DECIMAL(18,2)   NOT NULL,
    Stock                INT             NOT NULL DEFAULT 0,
    CategoriaId          INT             NOT NULL,
    ImagenUrl            NVARCHAR(500)   NULL,
    EsPromocion          BIT             NOT NULL DEFAULT 0,
    PrecioPromocion      DECIMAL(18,2)   NULL,
    FechaInicioPromocion DATETIME2       NULL,
    FechaFinPromocion    DATETIME2       NULL,
    Activo               BIT             NOT NULL DEFAULT 1,
    CreadoPorId          INT             NOT NULL,
    FechaCreacion        DATETIME2       NOT NULL DEFAULT GETUTCDATE(),
    FechaModificacion    DATETIME2       NULL,
    CONSTRAINT FK_Productos_Categorias FOREIGN KEY (CategoriaId) REFERENCES Categorias(Id),
    CONSTRAINT FK_Productos_Usuarios   FOREIGN KEY (CreadoPorId) REFERENCES Usuarios(Id)
);
CREATE INDEX IX_Productos_CategoriaId ON Productos(CategoriaId);
CREATE INDEX IX_Productos_Activo      ON Productos(Activo);
CREATE INDEX IX_Productos_EsPromocion ON Productos(EsPromocion);
GO

-- ── Ventas ────────────────────────────────────────────────
CREATE TABLE Ventas (
    Id            INT           IDENTITY(1,1) PRIMARY KEY,
    NumeroVenta   NVARCHAR(20)  NOT NULL UNIQUE,
    ClienteId     INT           NOT NULL,
    ColaboradorId INT           NOT NULL,
    Total         DECIMAL(18,2) NOT NULL,
    Observacion   NVARCHAR(500) NULL,
    Estado        NVARCHAR(50)  NOT NULL DEFAULT 'Completada'
                  CHECK (Estado IN ('Pendiente','Completada','Anulada')),
    FechaVenta    DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Ventas_Cliente      FOREIGN KEY (ClienteId)     REFERENCES Usuarios(Id),
    CONSTRAINT FK_Ventas_Colaborador  FOREIGN KEY (ColaboradorId) REFERENCES Usuarios(Id)
);
CREATE INDEX IX_Ventas_ClienteId     ON Ventas(ClienteId);
CREATE INDEX IX_Ventas_ColaboradorId ON Ventas(ColaboradorId);
CREATE INDEX IX_Ventas_FechaVenta    ON Ventas(FechaVenta);
GO

-- ── DetalleVentas ─────────────────────────────────────────
CREATE TABLE DetalleVentas (
    Id             INT           IDENTITY(1,1) PRIMARY KEY,
    VentaId        INT           NOT NULL,
    ProductoId     INT           NOT NULL,
    Cantidad       INT           NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal       DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_DetalleVentas_Venta    FOREIGN KEY (VentaId)    REFERENCES Ventas(Id),
    CONSTRAINT FK_DetalleVentas_Producto FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);
CREATE INDEX IX_DetalleVentas_VentaId    ON DetalleVentas(VentaId);
CREATE INDEX IX_DetalleVentas_ProductoId ON DetalleVentas(ProductoId);
GO

-- ── Auditoria ─────────────────────────────────────────────
CREATE TABLE Auditoria (
    Id              BIGINT         IDENTITY(1,1) PRIMARY KEY,
    UsuarioId       INT            NULL,
    UsuarioEmail    NVARCHAR(200)  NULL,
    Accion          NVARCHAR(100)  NOT NULL,
    Entidad         NVARCHAR(100)  NOT NULL,
    EntidadId       NVARCHAR(50)   NULL,
    DatosAnteriores NVARCHAR(MAX)  NULL,
    DatosNuevos     NVARCHAR(MAX)  NULL,
    IpAddress       NVARCHAR(50)   NULL,
    UserAgent       NVARCHAR(500)  NULL,
    FechaAccion     DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
    Exitoso         BIT            NOT NULL DEFAULT 1,
    MensajeError    NVARCHAR(1000) NULL
);
CREATE INDEX IX_Auditoria_UsuarioId   ON Auditoria(UsuarioId);
CREATE INDEX IX_Auditoria_FechaAccion ON Auditoria(FechaAccion);
CREATE INDEX IX_Auditoria_Accion      ON Auditoria(Accion);
GO

-- ── Vista de ventas completas ─────────────────────────────
CREATE OR ALTER VIEW vw_VentasCompletas AS
SELECT
    v.Id, v.NumeroVenta, v.FechaVenta, v.Total, v.Estado, v.Observacion,
    c.Nombre   + ' ' + c.Apellido   AS NombreCliente,   c.Email AS EmailCliente,
    col.Nombre + ' ' + col.Apellido AS NombreColaborador,
    (SELECT COUNT(*) FROM DetalleVentas dv WHERE dv.VentaId = v.Id) AS TotalItems
FROM Ventas v
INNER JOIN Usuarios c   ON v.ClienteId     = c.Id
INNER JOIN Usuarios col ON v.ColaboradorId = col.Id;
GO

PRINT '✅ RotterDB creada exitosamente.';
PRINT 'NOTA: Registra un usuario via API y luego ejecuta:';
PRINT '  UPDATE Usuarios SET RolId = 1 WHERE Email = ''tu@email.com'';';
GO
