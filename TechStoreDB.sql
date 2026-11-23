/*
  TechStoreDB.sql
  Crea la base de datos TechStoreDB para el sistema de inventario y ventas de TechStore.
  - 5 tablas con claves primarias, claves foráneas, valores por defecto, UNIQUE y CHECK.
  - Datos de prueba: 8 categorías, 500 clientes y 50,000 productos.
  ATENCIÓN: si TechStoreDB ya existe, este script la ELIMINA y la vuelve a crear.
*/
USE master;
GO

IF DB_ID(N'TechStoreDB') IS NOT NULL
BEGIN
    ALTER DATABASE TechStoreDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TechStoreDB;
END
GO

CREATE DATABASE TechStoreDB;
GO

USE TechStoreDB;
GO

/* ===================== TABLAS ===================== */

CREATE TABLE dbo.Categorias (
    CategoriaId INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Categorias PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL
        CONSTRAINT UQ_Categorias_Nombre UNIQUE
);
GO

CREATE TABLE dbo.Productos (
    ProductoId    INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Productos PRIMARY KEY,
    Nombre        NVARCHAR(150) NOT NULL,
    Sku           NVARCHAR(30)  NOT NULL
        CONSTRAINT UQ_Productos_Sku UNIQUE,
    CategoriaId   INT NOT NULL
        CONSTRAINT FK_Productos_Categorias REFERENCES dbo.Categorias (CategoriaId),
    Precio        DECIMAL(10,2) NOT NULL
        CONSTRAINT CK_Productos_Precio CHECK (Precio > 0),
    Stock         INT NOT NULL
        CONSTRAINT CK_Productos_Stock CHECK (Stock >= 0),
    FechaCreacion DATETIME2 NOT NULL
        CONSTRAINT DF_Productos_FechaCreacion DEFAULT SYSDATETIME(),
    Activo        BIT NOT NULL
        CONSTRAINT DF_Productos_Activo DEFAULT 1
);
GO

CREATE INDEX IX_Productos_Nombre      ON dbo.Productos (Nombre);
CREATE INDEX IX_Productos_CategoriaId ON dbo.Productos (CategoriaId);
GO

CREATE TABLE dbo.Clientes (
    ClienteId INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Clientes PRIMARY KEY,
    Nombre    NVARCHAR(100) NOT NULL,
    Email     NVARCHAR(150) NOT NULL
        CONSTRAINT UQ_Clientes_Email UNIQUE
);
GO

CREATE TABLE dbo.Ventas (
    VentaId   INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_Ventas PRIMARY KEY,
    ClienteId INT NOT NULL
        CONSTRAINT FK_Ventas_Clientes REFERENCES dbo.Clientes (ClienteId),
    Fecha     DATETIME2 NOT NULL
        CONSTRAINT DF_Ventas_Fecha DEFAULT SYSDATETIME(),
    Total     DECIMAL(12,2) NOT NULL
        CONSTRAINT CK_Ventas_Total CHECK (Total >= 0)
);
GO

CREATE INDEX IX_Ventas_ClienteId ON dbo.Ventas (ClienteId);
GO

CREATE TABLE dbo.DetalleVentas (
    DetalleId      INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_DetalleVentas PRIMARY KEY,
    VentaId        INT NOT NULL
        CONSTRAINT FK_DetalleVentas_Ventas REFERENCES dbo.Ventas (VentaId),
    ProductoId     INT NOT NULL
        CONSTRAINT FK_DetalleVentas_Productos REFERENCES dbo.Productos (ProductoId),
    Cantidad       INT NOT NULL
        CONSTRAINT CK_DetalleVentas_Cantidad CHECK (Cantidad > 0),
    PrecioUnitario DECIMAL(10,2) NOT NULL
        CONSTRAINT CK_DetalleVentas_Precio CHECK (PrecioUnitario > 0)
);
GO

CREATE INDEX IX_DetalleVentas_VentaId    ON dbo.DetalleVentas (VentaId);
CREATE INDEX IX_DetalleVentas_ProductoId ON dbo.DetalleVentas (ProductoId);
GO

/* ===================== DATOS DE PRUEBA ===================== */

INSERT INTO dbo.Categorias (Nombre) VALUES
    (N'Laptops'), (N'Smartphones'), (N'Tablets'), (N'Monitores'),
    (N'Teclados y ratones'), (N'Audio'), (N'Almacenamiento'), (N'Redes');
GO

-- 500 clientes
;WITH N AS (
    SELECT TOP (500) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.Clientes (Nombre, Email)
SELECT CONCAT(N'Cliente ', n), CONCAT(N'cliente', n, N'@techstore.test')
FROM N;
GO

-- 50,000 productos (reparten por igual entre las 8 categorías)
;WITH N AS (
    SELECT TOP (50000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects a CROSS JOIN sys.all_objects b
)
INSERT INTO dbo.Productos (Nombre, Sku, CategoriaId, Precio, Stock)
SELECT
    CONCAT(c.Nombre, N' modelo ', N.n),
    CONCAT(N'SKU-', RIGHT(CONCAT(N'000000', N.n), 6)),
    c.CategoriaId,
    CAST(10 + ((N.n * 37) % 99000) / 100.0 AS DECIMAL(10,2)),
    5 + (N.n % 500)
FROM N
JOIN dbo.Categorias c ON c.CategoriaId = 1 + (N.n % 8);
GO

/* ===================== VERIFICACIÓN ===================== */

SELECT 'Categorias' AS Tabla, COUNT(*) AS Filas FROM dbo.Categorias
UNION ALL SELECT 'Productos',     COUNT(*) FROM dbo.Productos
UNION ALL SELECT 'Clientes',      COUNT(*) FROM dbo.Clientes
UNION ALL SELECT 'Ventas',        COUNT(*) FROM dbo.Ventas
UNION ALL SELECT 'DetalleVentas', COUNT(*) FROM dbo.DetalleVentas;

SELECT COUNT(*) AS Restricciones_CHECK FROM sys.check_constraints;
SELECT COUNT(*) AS Claves_foraneas     FROM sys.foreign_keys;
GO
