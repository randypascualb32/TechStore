using System.Data;
using Microsoft.Data.SqlClient;
using TechStore.Core;

namespace TechStore.AdoNet;

public class AdoProductoRepository : IProductoRepository
{
    private const string Columnas = "ProductoId, Nombre, Sku, CategoriaId, Precio, Stock, FechaCreacion, Activo";

    private const string SqlInsertar =
        "INSERT INTO Productos (Nombre, Sku, CategoriaId, Precio, Stock, Activo) VALUES (@n, @s, @c, @p, @st, @a)";

    private readonly string _cs;

    public AdoProductoRepository(string connectionString) => _cs = connectionString;

    // Convierte una fila del DataReader en un objeto Producto (a mano).
    private static Producto Mapear(SqlDataReader dr) => new()
    {
        ProductoId = dr.GetInt32(0),
        Nombre = dr.GetString(1),
        Sku = dr.GetString(2),
        CategoriaId = dr.GetInt32(3),
        Precio = dr.GetDecimal(4),
        Stock = dr.GetInt32(5),
        FechaCreacion = dr.GetDateTime(6),
        Activo = dr.GetBoolean(7)
    };

    public Producto? ObtenerPorId(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand($"SELECT {Columnas} FROM Productos WHERE ProductoId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        cn.Open();
        using var dr = cmd.ExecuteReader();
        return dr.Read() ? Mapear(dr) : null;
    }

    public List<Producto> ObtenerTodos(int limite)
    {
        var lista = new List<Producto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand($"SELECT TOP (@n) {Columnas} FROM Productos ORDER BY ProductoId", cn);
        cmd.Parameters.AddWithValue("@n", limite);
        cn.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read()) lista.Add(Mapear(dr));
        return lista;
    }

    public List<Producto> Buscar(int categoriaId, decimal precioMin, decimal precioMax, int limite)
    {
        var lista = new List<Producto>();
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            $"SELECT TOP (@n) {Columnas} FROM Productos " +
            "WHERE CategoriaId = @c AND Precio >= @min AND Precio <= @max ORDER BY Precio, ProductoId", cn);
        cmd.Parameters.AddWithValue("@n", limite);
        cmd.Parameters.AddWithValue("@c", categoriaId);
        cmd.Parameters.Add(SqlHelper.Dinero("@min", precioMin));
        cmd.Parameters.Add(SqlHelper.Dinero("@max", precioMax));
        cn.Open();
        using var dr = cmd.ExecuteReader();
        while (dr.Read()) lista.Add(Mapear(dr));
        return lista;
    }

    public int Insertar(Producto p)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(SqlInsertar + "; SELECT CAST(SCOPE_IDENTITY() AS INT);", cn);
        cmd.Parameters.AddWithValue("@n", p.Nombre);
        cmd.Parameters.AddWithValue("@s", p.Sku);
        cmd.Parameters.AddWithValue("@c", p.CategoriaId);
        cmd.Parameters.Add(SqlHelper.Dinero("@p", p.Precio));
        cmd.Parameters.AddWithValue("@st", p.Stock);
        cmd.Parameters.AddWithValue("@a", p.Activo);
        cn.Open();
        return (int)cmd.ExecuteScalar()!;
    }

    public int InsertarVarios(IReadOnlyList<Producto> productos)
    {
        using var cn = new SqlConnection(_cs);
        cn.Open();
        using var tx = cn.BeginTransaction();
        try
        {
            using var cmd = new SqlCommand(SqlInsertar, cn, tx);
            var pN = cmd.Parameters.Add("@n", SqlDbType.NVarChar, 150);
            var pS = cmd.Parameters.Add("@s", SqlDbType.NVarChar, 30);
            var pC = cmd.Parameters.Add("@c", SqlDbType.Int);
            var pP = cmd.Parameters.Add(SqlHelper.Dinero("@p", 0));
            var pSt = cmd.Parameters.Add("@st", SqlDbType.Int);
            var pA = cmd.Parameters.Add("@a", SqlDbType.Bit);

            foreach (var p in productos)
            {
                pN.Value = p.Nombre;
                pS.Value = p.Sku;
                pC.Value = p.CategoriaId;
                pP.Value = p.Precio;
                pSt.Value = p.Stock;
                pA.Value = p.Activo;
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return productos.Count;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public bool Actualizar(Producto p)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "UPDATE Productos SET Nombre = @n, Sku = @s, CategoriaId = @c, Precio = @p, Stock = @st, Activo = @a " +
            "WHERE ProductoId = @id", cn);
        cmd.Parameters.AddWithValue("@n", p.Nombre);
        cmd.Parameters.AddWithValue("@s", p.Sku);
        cmd.Parameters.AddWithValue("@c", p.CategoriaId);
        cmd.Parameters.Add(SqlHelper.Dinero("@p", p.Precio));
        cmd.Parameters.AddWithValue("@st", p.Stock);
        cmd.Parameters.AddWithValue("@a", p.Activo);
        cmd.Parameters.AddWithValue("@id", p.ProductoId);
        cn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM Productos WHERE ProductoId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        cn.Open();
        return cmd.ExecuteNonQuery() > 0;
    }

    public int AjustarStockPorCategoria(int categoriaId, int delta)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand(
            "UPDATE Productos SET Stock = Stock + @d WHERE CategoriaId = @c AND Stock + @d >= 0", cn);
        cmd.Parameters.AddWithValue("@d", delta);
        cmd.Parameters.AddWithValue("@c", categoriaId);
        cn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int EliminarPorPrefijoSku(string prefijo)
    {
        using var cn = new SqlConnection(_cs);
        using var cmd = new SqlCommand("DELETE FROM Productos WHERE Sku LIKE @p + '%'", cn);
        cmd.Parameters.AddWithValue("@p", prefijo);
        cn.Open();
        return cmd.ExecuteNonQuery();
    }
}
