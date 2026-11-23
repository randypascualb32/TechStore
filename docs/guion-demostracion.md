# Guion de la demostración (unos 5 minutos)

1. **Idea (30 s).** "Construí el mismo módulo de inventario y ventas dos veces, con ADO.NET y con Entity Framework Core, sobre la misma base de datos y las mismas interfaces. Medí rendimiento y esfuerzo para recomendar una."
2. **Base de datos (30 s).** Mostrar el resultado de `TechStoreDB.sql`: 50,000 productos, restricciones CHECK, claves foráneas.
3. **Estructura (30 s).** Abrir el repositorio: `Core` (interfaces), `AdoNet`, `EfCore` (Code First y `Migrations`), `EfDbFirst` (modelo generado).
4. **Demostración en vivo (1.5 min).** `dotnet run -c Release --project src/TechStore.Benchmarks -- --demo`. Señalar la venta correcta y la venta rechazada por falta de stock, donde el stock no cambia (transacción).
5. **Resultados (1.5 min).** Mostrar `resultados.md`: dónde ADO.NET gana, dónde la diferencia es pequeña, qué cambia con `AsNoTracking`.
6. **Recomendación (1 min).** Basada en tus números y en las métricas de desarrollo: cuándo conviene ADO.NET, cuándo EF Core y la opción de combinarlos.

Consejo: ten `resultados.md` y la demostración ya ejecutados una vez antes de la clase, por si falla la conexión durante la exposición.
