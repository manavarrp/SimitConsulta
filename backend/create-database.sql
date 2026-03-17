-- =============================================================
-- create-database.sql
-- Ejecutado por Docker al arrancar el contenedor de SQL Server.
-- Única responsabilidad: crear la base de datos vacía.
-- Las tablas, índices y relaciones las crea EF Core (MigrateAsync).
-- =============================================================

USE master;
GO

IF NOT EXISTS (
    SELECT name FROM sys.databases
    WHERE name = 'SimitConsultaDB'
)
BEGIN
    CREATE DATABASE SimitConsultaDB
        COLLATE Latin1_General_CI_AS;

    PRINT 'Database SimitConsultaDB created.';
END
ELSE
BEGIN
    PRINT 'Database SimitConsultaDB already exists — no changes.';
END
GO