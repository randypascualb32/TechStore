# Genera (o regenera) el modelo Database First a partir de TechStoreDB.
# Ejecutar desde la raiz del repositorio:  .\scripts\generar-dbfirst.ps1
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$proj = Join-Path $root "src\TechStore.EfDbFirst"
$cs   = "Server=localhost\SQLEXPRESS;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"

if (-not (Test-Path $proj)) {
    dotnet new console -n TechStore.EfDbFirst -o $proj
    dotnet add $proj package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.0
    dotnet add $proj package Microsoft.EntityFrameworkCore.Design --version 10.0.0
    dotnet sln (Join-Path $root "TechStore.sln") add $proj
    # NuGetAudit desactivado para evitar avisos de seguridad en pantalla
    (Get-Content "$proj\TechStore.EfDbFirst.csproj") -replace '</PropertyGroup>', "  <NuGetAudit>false</NuGetAudit>`n  </PropertyGroup>" |
        Set-Content "$proj\TechStore.EfDbFirst.csproj"
}

# Esto es lo que GENERA el modelo: clases de entidad + DbContext a partir de las tablas existentes.
dotnet ef dbcontext scaffold $cs Microsoft.EntityFrameworkCore.SqlServer `
    --project $proj --output-dir Models --context TechStoreDbContext --no-onconfiguring --force

# Programa de demostración que usa el modelo generado
@'
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
'@ | Set-Content (Join-Path $proj "Program.cs")

Write-Host "Listo. Ejecuta: dotnet run --project src\TechStore.EfDbFirst"
