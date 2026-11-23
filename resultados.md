# Resultados de las pruebas de rendimiento

- Fecha: 2026-11-23 08:52
- Sistema: Microsoft Windows 10.0.26200 (2 procesadores lógicos)
- Plataforma: .NET 10.0.12
- Configuración: Release
- Productos en TechStoreDB: 50,000
- Modo: normal
- Cada escenario se ejecuta una vez de calentamiento (no cuenta) y luego varias repeticiones; se reporta el promedio.

## Tiempo promedio (milisegundos, menos es mejor)

| Escenario | ADO.NET | EF Core | EF Core NoTracking | EF Core / ADO.NET | EF Core NoTracking / ADO.NET |
|---|---:|---:|---:|---:|---:|
| Lectura por Id (500 consultas) | 361.3 | 1,203.5 | 476.9 | 3.33x | 1.32x |
| Listado de 100 productos | 1.5 | 2.6 | 4.4 | 1.81x | 3.00x |
| Listado de 1,000 productos | 10.2 | 12.6 | 10.5 | 1.23x | 1.03x |
| Listado de 10,000 productos | 29.4 | 63.0 | 32.2 | 2.14x | 1.09x |
| Listado de 50,000 productos | 161.5 | 449.6 | 159.1 | 2.78x | 0.99x |
| Búsqueda por categoría y precio (hasta 1,000 filas) | 19.1 | 23.3 | 24.5 | 1.22x | 1.28x |
| Inserción de 100 productos | 50.9 | 64.4 | 109.4 | 1.26x | 2.15x |
| Inserción de 1,000 productos | 613.1 | 190.8 | 223.0 | 0.31x | 0.36x |
| Inserción de 5,000 productos | 1,819.6 | 748.6 | 805.3 | 0.41x | 0.44x |
| Actualización masiva de stock (una categoría, unas 6,250 filas, +1 y -1) | 629.6 | 845.7 | 1,197.4 | 1.34x | 1.90x |
| Venta transaccional (100 ventas de 3 ítems) | 296.8 | 615.1 | 540.3 | 2.07x | 1.82x |

## Memoria asignada por ejecución (MB, menos es mejor; la relación se omite si ADO.NET usa menos de 0.05 MB)

| Escenario | ADO.NET | EF Core | EF Core NoTracking | EF Core / ADO.NET | EF Core NoTracking / ADO.NET |
|---|---:|---:|---:|---:|---:|
| Lectura por Id (500 consultas) | 2.72 | 47.17 | 46.51 | 17.36x | 17.12x |
| Listado de 100 productos | 0.03 | 0.28 | 0.19 | - | - |
| Listado de 1,000 productos | 0.20 | 1.41 | 0.53 | 7.06x | 2.62x |
| Listado de 10,000 productos | 2.05 | 12.77 | 4.03 | 6.22x | 1.96x |
| Listado de 50,000 productos | 10.11 | 61.89 | 19.41 | 6.12x | 1.92x |
| Búsqueda por categoría y precio (hasta 1,000 filas) | 0.20 | 1.42 | 0.53 | 7.11x | 2.67x |
| Inserción de 100 productos | 0.13 | 1.30 | 1.30 | 10.31x | 10.29x |
| Inserción de 1,000 productos | 1.22 | 11.41 | 11.38 | 9.36x | 9.34x |
| Inserción de 5,000 productos | 6.07 | 56.26 | 56.26 | 9.27x | 9.27x |
| Actualización masiva de stock (una categoría, unas 6,250 filas, +1 y -1) | 0.01 | 93.36 | 93.36 | - | - |
| Venta transaccional (100 ventas de 3 ítems) | 1.89 | 19.46 | 19.46 | 10.32x | 10.32x |
