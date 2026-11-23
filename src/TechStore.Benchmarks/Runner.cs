using System.Diagnostics;
using TechStore.Core;

namespace TechStore.Benchmarks;

public record Medicion(string Escenario, string Contendiente, double MediaMs, double MinMs, double MbAsignados);

public class Contendiente
{
    public string Nombre { get; }
    public IProductoRepository Productos { get; }
    public IVentaRepository Ventas { get; }

    public Contendiente(string nombre, IProductoRepository productos, IVentaRepository ventas)
    {
        Nombre = nombre;
        Productos = productos;
        Ventas = ventas;
    }
}

public static class Runner
{
    /// <summary>
    /// Ejecuta la acción una vez de calentamiento (no cuenta) y luego "repeticiones" veces midiendo
    /// con Stopwatch. La limpieza (si hay) se ejecuta fuera del tiempo medido.
    /// </summary>
    public static Medicion Medir(string escenario, string contendiente, Action accion, int repeticiones, Action? limpiar = null)
    {
        Console.Write($"  {contendiente,-22} {escenario} ... ");

        accion();
        limpiar?.Invoke();

        var tiempos = new List<double>();
        long bytes = 0;
        for (int i = 0; i < repeticiones; i++)
        {
            GC.Collect();
            long antes = GC.GetTotalAllocatedBytes(true);
            var sw = Stopwatch.StartNew();
            accion();
            sw.Stop();
            bytes += GC.GetTotalAllocatedBytes(true) - antes;
            tiempos.Add(sw.Elapsed.TotalMilliseconds);
            limpiar?.Invoke();
        }

        var m = new Medicion(escenario, contendiente, tiempos.Average(), tiempos.Min(),
            bytes / (double)repeticiones / 1024.0 / 1024.0);
        Console.WriteLine($"{m.MediaMs:N1} ms");
        return m;
    }
}
