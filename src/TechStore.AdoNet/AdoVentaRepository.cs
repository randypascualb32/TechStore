using Microsoft.Data.SqlClient;
using TechStore.Core;

namespace TechStore.AdoNet;

public class AdoVentaRepository : IVentaRepository
{
    private readonly string _cs;

    public AdoVentaRepository(string connectionString) => _cs = connectionString;

    public int RegistrarVenta(int clienteId, IReadOnlyList<ItemVenta> items)
    {
        using var cn = new SqlConnection(_cs);
        cn.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            var lineas = new List<(int ProductoId, int Cantidad, decimal Precio)>();
            decimal total = 0;

            // 1) Descontar stock de cada producto. La condición Stock >= @c evita vender de más.
            foreach (var item in items)
            {
                using var upd = new SqlCommand(
                    "UPDATE Productos SET Stock = Stock - @c OUTPUT inserted.Precio " +
                    "WHERE ProductoId = @id AND Activo = 1 AND Stock >= @c", cn, tx);
                upd.Parameters.AddWithValue("@c", item.Cantidad);
                upd.Parameters.AddWithValue("@id", item.ProductoId);
                var precioObj = upd.ExecuteScalar();
                if (precioObj is null)
                    throw new InvalidOperationException(
                        $"Producto {item.ProductoId} inexistente, inactivo o sin stock suficiente.");

                var precio = Convert.ToDecimal(precioObj);
                lineas.Add((item.ProductoId, item.Cantidad, precio));
                total += precio * item.Cantidad;
            }

            // 2) Crear la venta.
            int ventaId;
            using (var insV = new SqlCommand(
                "INSERT INTO Ventas (ClienteId, Total) OUTPUT inserted.VentaId VALUES (@cl, @t)", cn, tx))
            {
                insV.Parameters.AddWithValue("@cl", clienteId);
                insV.Parameters.Add(SqlHelper.Dinero("@t", total, 12));
                ventaId = Convert.ToInt32(insV.ExecuteScalar());
            }

            // 3) Crear los detalles.
            foreach (var l in lineas)
            {
                using var insD = new SqlCommand(
                    "INSERT INTO DetalleVentas (VentaId, ProductoId, Cantidad, PrecioUnitario) " +
                    "VALUES (@v, @p, @c, @pu)", cn, tx);
                insD.Parameters.AddWithValue("@v", ventaId);
                insD.Parameters.AddWithValue("@p", l.ProductoId);
                insD.Parameters.AddWithValue("@c", l.Cantidad);
                insD.Parameters.Add(SqlHelper.Dinero("@pu", l.Precio));
                insD.ExecuteNonQuery();
            }

            tx.Commit();
            return ventaId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public Venta? ObtenerPorId(int ventaId)
    {
        using var cn = new SqlConnection(_cs);
        cn.Open();

        Venta? venta = null;
        using (var cmd = new SqlCommand(
            "SELECT VentaId, ClienteId, Fecha, Total FROM Ventas WHERE VentaId = @id", cn))
        {
            cmd.Parameters.AddWithValue("@id", ventaId);
            using var dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                venta = new Venta
                {
                    VentaId = dr.GetInt32(0),
                    ClienteId = dr.GetInt32(1),
                    Fecha = dr.GetDateTime(2),
                    Total = dr.GetDecimal(3)
                };
            }
        }

        if (venta is null) return null;

        using (var cmd = new SqlCommand(
            "SELECT DetalleId, VentaId, ProductoId, Cantidad, PrecioUnitario " +
            "FROM DetalleVentas WHERE VentaId = @id ORDER BY DetalleId", cn))
        {
            cmd.Parameters.AddWithValue("@id", ventaId);
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                venta.Detalles.Add(new DetalleVenta
                {
                    DetalleId = dr.GetInt32(0),
                    VentaId = dr.GetInt32(1),
                    ProductoId = dr.GetInt32(2),
                    Cantidad = dr.GetInt32(3),
                    PrecioUnitario = dr.GetDecimal(4)
                });
            }
        }

        return venta;
    }
}
