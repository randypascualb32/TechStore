using Microsoft.EntityFrameworkCore;
using TechStore.EfDbFirst.Models;

var cs = @"Server=localhost\SQLEXPRESS;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;";
var opciones = new DbContextOptionsBuilder<TechStoreDbContext>().UseSqlServer(cs).Options;

using var db = new TechStoreDbContext(opciones);
Console.WriteLine("Modelo Database First (generado con scaffold)");
Console.WriteLine($"Categorias: {db.Categorias.Count():N0}");
Console.WriteLine($"Productos:  {db.Productos.Count():N0}");
Console.WriteLine("Primeros 5 productos:");
foreach (var p in db.Productos.AsNoTracking().OrderBy(p => p.ProductoId).Take(5))
    Console.WriteLine($"  {p.ProductoId} | {p.Nombre} | {p.Precio:N2}");
