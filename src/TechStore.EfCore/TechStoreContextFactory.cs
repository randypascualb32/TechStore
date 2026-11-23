using Microsoft.EntityFrameworkCore.Design;
using TechStore.Core;

namespace TechStore.EfCore;

/// <summary>
/// Lo usan las herramientas "dotnet ef" para crear el contexto al generar migraciones.
/// Apunta a TechStoreDB_CodeFirst, una base NUEVA que las migraciones crean desde cero.
/// </summary>
public class TechStoreContextFactory : IDesignTimeDbContextFactory<TechStoreContext>
{
    public TechStoreContext CreateDbContext(string[] args) =>
        TechStoreContext.Crear(DbSettings.CodeFirstConnection);
}
