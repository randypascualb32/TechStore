using TechStore.Core;

namespace TechStore.Benchmarks;

/// <summary>Demostración corta para la clase: mismas operaciones con ambas tecnologías.</summary>
public static class Demo
{
    public static void Ejecutar(string cs, List<Contendiente> contendientes)
    {
        int maxVenta = Limpieza.MaxVentaId(cs);
        Console.WriteLine($"Productos en la base de datos: {Limpieza.ContarProductos(cs):N0}\n");

        foreach (var c in contendientes.Take(2)) // ADO.NET y EF Core
        {
            Console.WriteLine($"=== {c.Nombre} ===");

            var p = c.Productos.ObtenerPorId(10)!;
            Console.WriteLine($"Producto 10: {p.Nombre} | precio {p.Precio:N2} | stock {p.Stock}");

            var busqueda = c.Productos.Buscar(3, 100m, 800m, 5);
            Console.WriteLine($"Búsqueda (categoría 3, precio 100-800), primeros {busqueda.Count}:");
            foreach (var b in busqueda) Console.WriteLine($"   {b.ProductoId} | {b.Nombre} | {b.Precio:N2}");

            // Venta correcta
            int stockAntes = c.Productos.ObtenerPorId(10)!.Stock;
            int ventaId = c.Ventas.RegistrarVenta(1, new[] { new ItemVenta(10, 2), new ItemVenta(11, 1) });
            var venta = c.Ventas.ObtenerPorId(ventaId)!;
            Console.WriteLine($"Venta #{ventaId} registrada: total {venta.Total:N2}, {venta.Detalles.Count} detalles.");
            Console.WriteLine($"Stock del producto 10: {stockAntes} -> {c.Productos.ObtenerPorId(10)!.Stock}");

            // Venta que falla: el segundo producto no tiene stock suficiente => nada se guarda
            int stockAntesFallo = c.Productos.ObtenerPorId(10)!.Stock;
            try
            {
                c.Ventas.RegistrarVenta(1, new[] { new ItemVenta(10, 1), new ItemVenta(11, 100000) });
                Console.WriteLine("ERROR: la venta debió fallar.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Venta rechazada (esperado): {ex.Message}");
                int stockDespues = c.Productos.ObtenerPorId(10)!.Stock;
                Console.WriteLine(stockDespues == stockAntesFallo
                    ? $"Transacción correcta: el stock del producto 10 sigue en {stockDespues} (no se guardó nada)."
                    : "ERROR: el stock cambió, la transacción no deshizo los cambios.");
            }

            Limpieza.RestaurarVentas(cs, maxVenta);
            Console.WriteLine();
        }
    }
}
