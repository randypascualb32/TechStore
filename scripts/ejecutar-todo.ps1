# Ejecuta TODA la practica en orden y guarda la evidencia en la carpeta "evidencias".
# Uso:   powershell -ExecutionPolicy Bypass -File .\scripts\ejecutar-todo.ps1
# Prueba rapida:                 ... ejecutar-todo.ps1 -Rapido
# Retomar desde un paso (1 a 7): ... ejecutar-todo.ps1 -Desde 4
param([switch]$Rapido, [int]$Desde = 0)

$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
Set-Location (Split-Path -Parent $PSScriptRoot)
$srv = "localhost\SQLEXPRESS"
New-Item -ItemType Directory -Force evidencias | Out-Null

function Paso($texto) { Write-Host "`n=== $texto ===" -ForegroundColor Cyan }
function Verificar($paso) { if ($LASTEXITCODE -ne 0) { throw "Fallo el paso: $paso (codigo $LASTEXITCODE). Revisa el mensaje de arriba." } }

Paso "0. Herramienta dotnet-ef"
if (-not (dotnet tool list --global | Select-String "dotnet-ef")) {
    dotnet tool install --global dotnet-ef --version 10.0.0
    Verificar "instalar dotnet-ef"
}

if ($Desde -le 1) {
    Paso "1. Crear TechStoreDB con 50,000 productos"
    sqlcmd -S $srv -E -C -b -i TechStoreDB.sql | Tee-Object evidencias\01-base-de-datos.txt
    Verificar "crear la base de datos"
}

if ($Desde -le 2) {
    Paso "2. Compilar la solucion (Release)"
    dotnet build -c Release 2>&1 | Tee-Object evidencias\02-compilacion.txt
    Verificar "compilar"
}

if ($Desde -le 3) {
    Paso "3. Code First: migracion y base de datos TechStoreDB_CodeFirst"
    if (-not (Test-Path "src\TechStore.EfCore\Migrations")) {
        dotnet ef migrations add Inicial --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks | Tee-Object evidencias\03a-migracion.txt
        Verificar "crear la migracion"
    }
    dotnet ef database update --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks | Tee-Object evidencias\03b-database-update.txt
    Verificar "aplicar la migracion"
    sqlcmd -S $srv -E -C -b -d TechStoreDB_CodeFirst -W -Q "SELECT name AS Tablas_creadas_por_la_migracion FROM sys.tables" | Tee-Object evidencias\03c-tablas-code-first.txt
    Verificar "comprobar las tablas de Code First"
}

if ($Desde -le 4) {
    Paso "4. Database First: generar el modelo desde TechStoreDB"
    & "$PSScriptRoot\generar-dbfirst.ps1" | Tee-Object evidencias\04a-scaffold.txt
    dotnet run --project src/TechStore.EfDbFirst | Tee-Object evidencias\04b-database-first.txt
    Verificar "ejecutar el modelo Database First"
}

if ($Desde -le 5) {
    Paso "5. Demostracion: venta transaccional con ADO.NET y EF Core"
    dotnet run -c Release --project src/TechStore.Benchmarks -- --demo | Tee-Object evidencias\05-demostracion.txt
    Verificar "demostracion"
}

if ($Desde -le 6) {
    Paso "6. Pruebas de rendimiento (tarda varios minutos; no uses la computadora para otra cosa)"
    if ($Rapido) {
        dotnet run -c Release --project src/TechStore.Benchmarks -- --rapido | Tee-Object evidencias\06-rendimiento.txt
    } else {
        dotnet run -c Release --project src/TechStore.Benchmarks | Tee-Object evidencias\06-rendimiento.txt
    }
    Verificar "pruebas de rendimiento"
}

Paso "7. Conteo de lineas de codigo"
& "$PSScriptRoot\contar-lineas.ps1" | Tee-Object evidencias\07-lineas-de-codigo.txt

Write-Host "`nListo. Evidencias en la carpeta 'evidencias' y resultados en resultados.md y resultados.csv." -ForegroundColor Green
