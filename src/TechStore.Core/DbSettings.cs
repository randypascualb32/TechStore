namespace TechStore.Core;

/// <summary>
/// Cadenas de conexión. Por defecto apuntan a SQL Server Express local.
/// Se pueden cambiar con las variables de entorno TECHSTORE_CONN y TECHSTORE_CF_CONN.
/// </summary>
public static class DbSettings
{
    /// <summary>Base TechStoreDB (creada con TechStoreDB.sql). La usan todas las pruebas.</summary>
    public static string Connection =>
        Environment.GetEnvironmentVariable("TECHSTORE_CONN")
        ?? @"Server=localhost\SQLEXPRESS;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;";

    /// <summary>Base nueva que crean las migraciones de Code First (TechStoreDB_CodeFirst).</summary>
    public static string CodeFirstConnection =>
        Environment.GetEnvironmentVariable("TECHSTORE_CF_CONN")
        ?? @"Server=localhost\SQLEXPRESS;Database=TechStoreDB_CodeFirst;Trusted_Connection=True;TrustServerCertificate=True;";
}
