-- Deshace el cambio de cambio_agregar_marca.sql
USE TechStoreDB;
GO
ALTER TABLE dbo.Productos DROP COLUMN Marca;
GO
