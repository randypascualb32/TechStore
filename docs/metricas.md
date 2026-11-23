# Métricas de desarrollo

> Los datos de las tablas marcadas como **[completar]** deben ser los reales de tu trabajo. No inventes valores: el profesor puede preguntar cómo los obtuviste.

## 1. Líneas de código

Ejecuta `.\scripts\contar-lineas.ps1` (cuenta líneas sin vacías ni comentarios de una línea). Cifras del código entregado, antes de generar migraciones y Database First:

| Proyecto | Líneas escritas a mano |
|---|---:|
| TechStore.Core (modelos e interfaces, compartido) | 68 |
| TechStore.AdoNet (repositorios + ayuda SQL) | 257 |
| TechStore.EfCore (repositorios + modelo Code First) | 208 |

Detalle útil para el informe:

| | ADO.NET | EF Core |
|---|---:|---:|
| Repositorios de productos y ventas | 249 | 121 |
| Configuración del modelo (tablas, restricciones, índices) | 0 (está en el script SQL) | 87 |
| Código generado automáticamente | 0 | Migraciones y Database First **[completar con el script]** |

## 2. Tiempo y errores de desarrollo

Registra tiempos reales con un cronómetro (por ejemplo, el del teléfono) y anota cada error que encuentres.

| Tarea | ADO.NET | EF Core |
|---|---|---|
| Tiempo para dejar funcionando los repositorios (minutos) | [completar] | [completar] |
| Errores de compilación o ejecución encontrados | [completar] | [completar] |
| Tiempo en Code First: crear migración y base (minutos) | no aplica | [completar] |
| Tiempo en Database First: generar el modelo (minutos) | no aplica | [completar] |

Si un error se repite, cuéntalo una sola vez y anota la causa (por ejemplo: "nombre de servidor incorrecto en la cadena de conexión").

## 3. Esfuerzo ante un cambio en la base de datos

Cambio: agregar la columna `Marca NVARCHAR(50) NULL` a la tabla `Productos`. Mide cada caso con cronómetro y cuenta los archivos y líneas que tuviste que tocar (`git diff --stat` ayuda).

**ADO.NET**: aplica `scripts/cambio_agregar_marca.sql` y luego haz que el sistema use la columna:
1. Agregar `Marca` a la clase `Producto` (Core).
2. Agregar `Marca` a la lista de columnas del `SELECT` y a `Mapear` (leer con cuidado el valor nulo).
3. Agregar `Marca` al `INSERT` (`Insertar` e `InsertarVarios`) y al `UPDATE` (`Actualizar`) con su parámetro.

**EF Core Code First**:
1. Agregar `Marca` a `Producto` y configurarla en `TechStoreContext` (`HasMaxLength(50)`).
2. `dotnet ef migrations add AgregarMarca --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks`
3. `dotnet ef database update --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks`

**EF Core Database First**:
1. Aplicar `scripts/cambio_agregar_marca.sql`.
2. Volver a ejecutar `.\scripts\generar-dbfirst.ps1` (regenera el modelo con `--force`).

| | ADO.NET | Code First | Database First |
|---|---|---|---|
| Archivos modificados | [completar] | [completar] | [completar] |
| Líneas modificadas | [completar] | [completar] | [completar] |
| Tiempo (minutos) | [completar] | [completar] | [completar] |
| Errores | [completar] | [completar] | [completar] |

Para volver al estado original: `scripts/revertir_marca.sql` y deshacer los cambios con `git checkout .`.
