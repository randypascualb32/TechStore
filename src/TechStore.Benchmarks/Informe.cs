using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace TechStore.Benchmarks;

public static class Informe
{
    public static string Generar(List<Medicion> r, List<string> contendientes, long productos, bool rapido)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Resultados de las pruebas de rendimiento");
        sb.AppendLine();
        sb.AppendLine($"- Fecha: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"- Sistema: {RuntimeInformation.OSDescription} ({Environment.ProcessorCount} procesadores lógicos)");
        sb.AppendLine($"- Plataforma: .NET {Environment.Version}");
#if DEBUG
        sb.AppendLine("- Configuración: Debug (para resultados válidos ejecutar con -c Release)");
#else
        sb.AppendLine("- Configuración: Release");
#endif
        sb.AppendLine($"- Productos en TechStoreDB: {productos:N0}");
        sb.AppendLine($"- Modo: {(rapido ? "rápido (menos repeticiones)" : "normal")}");
        sb.AppendLine("- Cada escenario se ejecuta una vez de calentamiento (no cuenta) y luego varias repeticiones; se reporta el promedio.");
        sb.AppendLine();

        var escenarios = r.Select(x => x.Escenario).Distinct().ToList();

        sb.AppendLine("## Tiempo promedio (milisegundos, menos es mejor)");
        sb.AppendLine();
        Tabla(sb, r, escenarios, contendientes, m => m.MediaMs, "N1", conRelacion: true, minBase: 0);

        sb.AppendLine();
        sb.AppendLine("## Memoria asignada por ejecución (MB, menos es mejor; la relación se omite si ADO.NET usa menos de 0.05 MB)");
        sb.AppendLine();
        Tabla(sb, r, escenarios, contendientes, m => m.MbAsignados, "N2", conRelacion: true, minBase: 0.05);
        return sb.ToString();
    }

    private static void Tabla(StringBuilder sb, List<Medicion> r, List<string> escenarios, List<string> contendientes,
        Func<Medicion, double> valor, string formato, bool conRelacion, double minBase)
    {
        sb.Append("| Escenario |");
        foreach (var c in contendientes) sb.Append($" {c} |");
        if (conRelacion) foreach (var c in contendientes.Skip(1)) sb.Append($" {c} / {contendientes[0]} |");
        sb.AppendLine();

        sb.Append("|---|");
        for (int i = 0; i < contendientes.Count; i++) sb.Append("---:|");
        if (conRelacion) for (int i = 1; i < contendientes.Count; i++) sb.Append("---:|");
        sb.AppendLine();

        foreach (var esc in escenarios)
        {
            sb.Append($"| {esc} |");
            var valores = contendientes
                .Select(c => valor(r.First(x => x.Escenario == esc && x.Contendiente == c))).ToList();
            foreach (var v in valores) sb.Append($" {v.ToString(formato)} |");
            if (conRelacion)
                foreach (var v in valores.Skip(1))
                    sb.Append(valores[0] > minBase ? $" {(v / valores[0]).ToString("0.00")}x |" : " - |");
            sb.AppendLine();
        }
    }

    public static string Csv(List<Medicion> r)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.AppendLine("Escenario;Contendiente;MediaMs;MinMs;MbAsignados");
        foreach (var m in r)
            sb.AppendLine($"{m.Escenario};{m.Contendiente};{m.MediaMs.ToString("F2", inv)};{m.MinMs.ToString("F2", inv)};{m.MbAsignados.ToString("F2", inv)}");
        return sb.ToString();
    }
}
