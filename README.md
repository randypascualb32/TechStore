# TechStore: ADO.NET vs Entity Framework Core

Proyecto de la práctica **Bases de Datos y ORM**. TechStore necesita un sistema de inventario y ventas y quiere saber, con datos reales, qué tecnología de acceso a datos le conviene. El mismo módulo está implementado **dos veces** (ADO.NET y Entity Framework Core) sobre la **misma base de datos** y cumpliendo las **mismas interfaces**.

## Qué contiene

| Carpeta / archivo | Contenido |
|---|---|
| `TechStoreDB.sql` | Crea la base `TechStoreDB`: 5 tablas con PK, FK, UNIQUE, CHECK y DEFAULT, 8 categorías, 500 clientes y **50,000 productos** |
| `src/TechStore.Core` | Modelos (`Producto`, `Venta`, ...) e interfaces (`IProductoRepository`, `IVentaRepository`) |
| `src/TechStore.AdoNet` | Repositorios con ADO.NET: consultas parametrizadas y venta transaccional (`SqlTransaction`) |
| `src/TechStore.EfCore` | Repositorios con EF Core, modelo **Code First** y carpeta `Migrations` |
| `src/TechStore.EfDbFirst` | Modelo **Database First** generado con `scaffold` (se crea con `scripts/generar-dbfirst.ps1`) |
| `src/TechStore.Benchmarks` | Pruebas de rendimiento, demostración y generación de resultados |
| `scripts/` | Cambio de base de datos para medir esfuerzo, conteo de líneas y generador de Database First |
| `docs/metricas.md` | Métricas de desarrollo (líneas de código, tiempo, errores, cambio en la base de datos) |
| `docs/guion-demostracion.md` | Guion de la demostración en clase |

## Requisitos

- .NET SDK 10
- SQL Server Express (instancia `localhost\SQLEXPRESS`, autenticación de Windows)
- Herramientas: `sqlcmd` y `dotnet-ef`

```powershell
dotnet tool install --global dotnet-ef --version 10.0.0
```

Si tu servidor tiene otro nombre, define la variable de entorno `TECHSTORE_CONN` con tu cadena de conexión antes de ejecutar (y cambia `localhost\SQLEXPRESS` en los comandos).

## Atajo: ejecutar todo con un solo comando

```powershell
.\scripts\ejecutar-todo.ps1
```

Hace los pasos 1 a 7 en orden y guarda la salida de cada uno en la carpeta `evidencias/` (útil para las capturas). Para una primera prueba rápida: `.\scripts\ejecutar-todo.ps1 -Rapido`. Si PowerShell bloquea el script: `powershell -ExecutionPolicy Bypass -File .\scripts\ejecutar-todo.ps1`.

## Cómo ejecutarlo (en este orden)

Todos los comandos se ejecutan desde la raíz del repositorio.

### 1. Crear la base de datos

```powershell
sqlcmd -S localhost\SQLEXPRESS -E -C -i TechStoreDB.sql
```

Al final muestra el número de filas por tabla (debe haber 50,000 productos). **El script elimina y recrea `TechStoreDB` si ya existe.**

### 2. Compilar

```powershell
dotnet build -c Release
```

### 3. Code First con migraciones

Genera la migración y crea una base **nueva** (`TechStoreDB_CodeFirst`) solo con el modelo de C#:

```powershell
dotnet ef migrations add Inicial --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks
dotnet ef database update --project src/TechStore.EfCore --startup-project src/TechStore.Benchmarks
sqlcmd -S localhost\SQLEXPRESS -E -C -d TechStoreDB_CodeFirst -W -Q "SELECT name FROM sys.tables"
```

Debe listar `Categorias`, `Productos`, `Clientes`, `Ventas`, `DetalleVentas` y `__EFMigrationsHistory`. La carpeta `src/TechStore.EfCore/Migrations` se sube al repositorio.

### 4. Database First

Genera las clases y el `DbContext` leyendo `TechStoreDB`:

```powershell
.\scripts\generar-dbfirst.ps1
dotnet run --project src/TechStore.EfDbFirst
```

Si PowerShell bloquea el script: `powershell -ExecutionPolicy Bypass -File .\scripts\generar-dbfirst.ps1`.

### 5. Demostración (venta transaccional)

```powershell
dotnet run -c Release --project src/TechStore.Benchmarks -- --demo
```

Con cada tecnología: lee un producto, busca con filtros, registra una venta y comprueba que una venta con stock insuficiente **no guarda nada** (transacción).

### 6. Pruebas de rendimiento

```powershell
dotnet run -c Release --project src/TechStore.Benchmarks
```

- **Usa siempre `-c Release`** y cierra otros programas pesados: Debug da resultados no válidos.
- Tarda varios minutos. Para una prueba rápida de que todo funciona: agrega `-- --rapido`.
- Genera `resultados.md` (tablas de tiempo y memoria) y `resultados.csv` en la carpeta donde ejecutes el comando.

Escenarios medidos (cada uno con un calentamiento y varias repeticiones; se reporta el promedio):

1. Lectura por Id (500 consultas)
2. Listado de 100, 1,000, 10,000 y 50,000 productos
3. Búsqueda por categoría y rango de precio
4. Inserción de 100, 1,000 y 5,000 productos
5. Actualización masiva de stock (una categoría, unas 6,250 filas)
6. Venta transaccional (100 ventas de 3 ítems)

Se comparan tres contendientes: **ADO.NET**, **EF Core** (con tracking, uso por defecto) y **EF Core NoTracking** (lecturas con `AsNoTracking()`). Las pruebas dejan la base como estaba (borran lo insertado y restauran el stock).

### 7. Métricas de desarrollo

```powershell
.\scripts\contar-lineas.ps1
```

Ver `docs/metricas.md`.

## Notas de diseño (importantes para interpretar los resultados)

- Las dos implementaciones usan **exactamente las mismas interfaces** y la misma base de datos. Las pruebas solo conocen `IProductoRepository` e `IVentaRepository`.
- ADO.NET usa consultas parametrizadas en todo momento. La venta usa un `UPDATE ... WHERE Stock >= @cantidad` atómico, por lo que no puede vender de más aunque haya ventas simultáneas.
- En EF Core la venta carga los productos, descuenta stock y guarda todo con un solo `SaveChanges()`, que ya es una transacción. Esa forma no está protegida contra ventas simultáneas sin agregar un token de concurrencia (no se hizo para mantener la comparación simple).
- En la actualización masiva EF usa la forma habitual (cargar, modificar, guardar), mientras que ADO.NET usa un solo `UPDATE`. Es una diferencia real de cómo se programa cada tecnología, no un error de la prueba.
- Los repositorios crean una conexión o contexto nuevo por operación en ambos casos.
- Database First no se mide por separado: usa el mismo motor de EF Core que Code First.

## Subir a GitHub

```powershell
git init
git add .
git commit -m "TechStore: ADO.NET vs Entity Framework Core"
git branch -M main
git remote add origin https://github.com/TU_USUARIO/TechStore.git
git push -u origin main
```

Crea antes el repositorio **público** y vacío en github.com (sin README ni .gitignore) y reemplaza `TU_USUARIO`.
