using System.Data;
using Microsoft.Data.SqlClient;

namespace TechStore.AdoNet;

internal static class SqlHelper
{
    /// <summary>Parámetro decimal con precisión explícita (evita redondeos inesperados).</summary>
    public static SqlParameter Dinero(string nombre, decimal valor, byte precision = 10) =>
        new(nombre, SqlDbType.Decimal) { Precision = precision, Scale = 2, Value = valor };
}
