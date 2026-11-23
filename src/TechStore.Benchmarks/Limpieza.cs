using Microsoft.Data.SqlClient;

namespace TechStore.Benchmarks;

/// <summary>Utilidades de las pruebas: dejan la base de datos como estaba después de cada escenario.</summary>
public static class Limpieza
{
    public static long ContarProductos(string cs)
    {
        using var cn = new SqlConnection(cs);
        using var cmd = new SqlCommand("SELECT COUNT_BIG(*) FROM Productos", cn);
        cn.Open();
        return Convert.ToInt64(cmd.ExecuteScalar());
    }

    public static int MaxVentaId(string cs)
    {
        using var cn = new SqlConnection(cs);
        using var cmd = new SqlCommand("SELECT ISNULL(MAX(VentaId), 0) FROM Ventas", cn);
        cn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    /// <summary>Borra las ventas creadas después de maxIdAntes y devuelve el stock descontado.</summary>
    public static void RestaurarVentas(string cs, int maxIdAntes)
    {
        using var cn = new SqlConnection(cs);
        cn.Open();
        using var tx = cn.BeginTransaction();
        const string sql = @"
UPDATE p SET p.Stock = p.Stock + d.Cant
FROM Productos p
JOIN (SELECT ProductoId, SUM(Cantidad) AS Cant FROM DetalleVentas WHERE VentaId > @max GROUP BY ProductoId) d
  ON d.ProductoId = p.ProductoId;
DELETE FROM DetalleVentas WHERE VentaId > @max;
DELETE FROM Ventas WHERE VentaId > @max;";
        using var cmd = new SqlCommand(sql, cn, tx);
        cmd.Parameters.AddWithValue("@max", maxIdAntes);
        cmd.ExecuteNonQuery();
        tx.Commit();
    }
}
