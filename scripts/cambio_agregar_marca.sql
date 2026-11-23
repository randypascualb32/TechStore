-- Cambio en la base de datos para medir el esfuerzo: agregar la columna Marca a Productos.
USE TechStoreDB;
GO
ALTER TABLE dbo.Productos ADD Marca NVARCHAR(50) NULL;
GO
SELECT TOP (3) ProductoId, Nombre, Marca FROM dbo.Productos;
GO
