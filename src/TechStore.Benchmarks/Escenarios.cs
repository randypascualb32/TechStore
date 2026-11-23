using TechStore.Core;

namespace TechStore.Benchmarks;

public static class Escenarios
{
    private static Producto NuevoProducto(int i) => new()
    {
        Nombre = $"Producto de prueba {i}",
        Sku = $"BENCH-{i:D6}",
        CategoriaId = 1 + i % 8,
        Precio = 99.99m,
        Stock = 10,
        Activo = true
    };

    private static List<Producto> NuevosProductos(int n) =>
        Enumerable.Range(1, n).Select(NuevoProducto).ToList();

    public static IReadOnlyList<ItemVenta> ItemsVenta(int i)
    {
        int b = (i * 3) % 49000;
        return new[] { new ItemVenta(1 + b, 1), new ItemVenta(2 + b, 1), new ItemVenta(3 + b, 1) };
    }

    public static List<Medicion> Ejecutar(string cs, List<Contendiente> contendientes, bool rapido)
    {
        var res = new List<Medicion>();
        int R(int normal) => rapido ? Math.Max(1, normal / 3) : normal;

        // 1) Lectura de un registro por Id, 500 veces
        Console.WriteLine("\n[1] Lectura por Id");
        var ids = Enumerable.Range(0, 500).Select(i => 1 + (i * 97) % 50000).ToArray();
        foreach (var c in contendientes)
            res.Add(Runner.Medir("Lectura por Id (500 consultas)", c.Nombre,
                () => { foreach (var id in ids) c.Productos.ObtenerPorId(id); }, R(5)));

        // 2) Listado con distintos volúmenes
        Console.WriteLine("\n[2] Listado de productos (distintos volúmenes)");
        foreach (var (n, reps) in new[] { (100, 30), (1000, 15), (10000, 5), (50000, 3) })
            foreach (var c in contendientes)
                res.Add(Runner.Medir($"Listado de {n:N0} productos", c.Nombre,
                    () => c.Productos.ObtenerTodos(n), R(reps)));

        // 3) Búsqueda con filtros
        Console.WriteLine("\n[3] Búsqueda con filtros");
        foreach (var c in contendientes)
            res.Add(Runner.Medir("Búsqueda por categoría y precio (hasta 1,000 filas)", c.Nombre,
                () => c.Productos.Buscar(3, 100m, 800m, 1000), R(15)));

        // 4) Inserción con distintos volúmenes
        Console.WriteLine("\n[4] Inserción de productos (distintos volúmenes)");
        foreach (var n in new[] { 100, 1000, 5000 })
            foreach (var c in contendientes)
                res.Add(Runner.Medir($"Inserción de {n:N0} productos", c.Nombre,
                    () => c.Productos.InsertarVarios(NuevosProductos(n)), R(3),
                    () => c.Productos.EliminarPorPrefijoSku("BENCH-")));

        // 5) Actualización masiva (+1 y -1 deja los datos como estaban)
        Console.WriteLine("\n[5] Actualización masiva de stock");
        foreach (var c in contendientes)
            res.Add(Runner.Medir("Actualización masiva de stock (una categoría, unas 6,250 filas, +1 y -1)", c.Nombre,
                () =>
                {
                    c.Productos.AjustarStockPorCategoria(1, 1);
                    c.Productos.AjustarStockPorCategoria(1, -1);
                }, R(3)));

        // 6) Venta transaccional
        Console.WriteLine("\n[6] Venta transaccional");
        int maxVenta = Limpieza.MaxVentaId(cs);
        foreach (var c in contendientes)
            res.Add(Runner.Medir("Venta transaccional (100 ventas de 3 ítems)", c.Nombre,
                () =>
                {
                    for (int i = 0; i < 100; i++)
                        c.Ventas.RegistrarVenta(1 + i % 500, ItemsVenta(i));
                }, R(3),
                () => Limpieza.RestaurarVentas(cs, maxVenta)));

        return res;
    }
}
