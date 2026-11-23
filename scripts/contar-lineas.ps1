# Cuenta lineas de codigo (sin lineas vacias ni comentarios de una línea) por proyecto.
# "Manuales" = escritas por nosotros. "Generadas" = carpetas Migrations y Models (las crea EF).
$root = Split-Path -Parent $PSScriptRoot

function Contar($archivos) {
    if (-not $archivos) { return 0 }
    ($archivos | ForEach-Object { Get-Content $_.FullName } |
        Where-Object { $_.Trim() -ne '' -and -not $_.Trim().StartsWith('//') }).Count
}

Get-ChildItem (Join-Path $root "src") -Directory | ForEach-Object {
    $todos = @(Get-ChildItem $_.FullName -Recurse -Filter *.cs | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' })
    $gen = @($todos | Where-Object { $_.FullName -match '\\(Migrations|Models)\\' })
    $man = @($todos | Where-Object { $_.FullName -notmatch '\\(Migrations|Models)\\' })
    [PSCustomObject]@{
        Proyecto = $_.Name
        Archivos = $todos.Count
        LineasManuales = Contar $man
        LineasGeneradas = Contar $gen
    }
} | Format-Table -AutoSize
