using TechStore.AdoNet;
using TechStore.Benchmarks;
using TechStore.Core;
using TechStore.EfCore;

var cs = DbSettings.Connection;
bool rapido = args.Contains("--rapido");
bool demo = args.Contains("--demo");

Console.WriteLine("TechStore: ADO.NET vs Entity Framework Core");
Console.WriteLine($"Conexión: {cs}");

long productos;
try
{
    productos = Limpieza.ContarProductos(cs);
}
catch (Exception ex)
{
    Console.WriteLine($"No se pudo consultar TechStoreDB: {ex.Message}");
    Console.WriteLine("Ejecuta primero el script TechStoreDB.sql (ver README).");
    return 1;
}

if (productos < 50000)
    Console.WriteLine($"AVISO: la base tiene {productos:N0} productos; el escenario esperaba 50,000.");

var contendientes = new List<Contendiente>
{
    new("ADO.NET", new AdoProductoRepository(cs), new AdoVentaRepository(cs)),
    new("EF Core", new EfProductoRepository(cs), new EfVentaRepository(cs)),
    new("EF Core NoTracking", new EfProductoRepository(cs, soloLecturaSinTracking: true), new EfVentaRepository(cs))
};

if (demo)
{
    Demo.Ejecutar(cs, contendientes);
    return 0;
}

#if DEBUG
Console.WriteLine("AVISO: estás en Debug. Para resultados válidos usa: dotnet run -c Release --project src/TechStore.Benchmarks");
#endif

var resultados = Escenarios.Ejecutar(cs, contendientes, rapido);
var nombres = contendientes.Select(c => c.Nombre).ToList();

var md = Informe.Generar(resultados, nombres, productos, rapido);
File.WriteAllText("resultados.md", md);
File.WriteAllText("resultados.csv", Informe.Csv(resultados));

Console.WriteLine("\n" + md);
Console.WriteLine("Archivos generados: resultados.md y resultados.csv");
return 0;
