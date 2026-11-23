using Microsoft.EntityFrameworkCore;
using TechStore.Core;

namespace TechStore.EfCore;

public class EfProductoRepository : IProductoRepository
{
    private readonly string _cs;
    private readonly bool _sinTracking;

    /// <param name="soloLecturaSinTracking">
    /// true = las consultas de lectura usan AsNoTracking() (EF no vigila los objetos leídos).
    /// </param>
    public EfProductoRepository(string connectionString, bool soloLecturaSinTracking = false)
    {
        _cs = connectionString;
        _sinTracking = soloLecturaSinTracking;
    }

    private TechStoreContext Crear() => TechStoreContext.Crear(_cs);

    private IQueryable<Producto> Lectura(TechStoreContext db) =>
        _sinTracking ? db.Productos.AsNoTracking() : db.Productos;

    public Producto? ObtenerPorId(int id)
    {
        using var db = Crear();
        return Lectura(db).FirstOrDefault(p => p.ProductoId == id);
    }

    public List<Producto> ObtenerTodos(int limite)
    {
        using var db = Crear();
        return Lectura(db).OrderBy(p => p.ProductoId).Take(limite).ToList();
    }

    public List<Producto> Buscar(int categoriaId, decimal precioMin, decimal precioMax, int limite)
    {
        using var db = Crear();
        return Lectura(db)
            .Where(p => p.CategoriaId == categoriaId && p.Precio >= precioMin && p.Precio <= precioMax)
            .OrderBy(p => p.Precio).ThenBy(p => p.ProductoId)
            .Take(limite)
            .ToList();
    }

    public int Insertar(Producto producto)
    {
        using var db = Crear();
        db.Productos.Add(producto);
        db.SaveChanges();
        return producto.ProductoId;
    }

    public int InsertarVarios(IReadOnlyList<Producto> productos)
    {
        using var db = Crear();
        db.Productos.AddRange(productos);
        db.SaveChanges();
        return productos.Count;
    }

    public bool Actualizar(Producto producto)
    {
        using var db = Crear();
        db.Productos.Update(producto);
        db.Entry(producto).Property(p => p.FechaCreacion).IsModified = false; // la fecha de creación no se reescribe
        try
        {
            return db.SaveChanges() > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false; // la fila ya no existe
        }
    }

    public bool Eliminar(int id)
    {
        using var db = Crear();
        return db.Productos.Where(p => p.ProductoId == id).ExecuteDelete() > 0;
    }

    public int AjustarStockPorCategoria(int categoriaId, int delta)
    {
        using var db = Crear();
        // Forma habitual en EF: cargar las entidades, modificarlas y guardar los cambios.
        var productos = db.Productos
            .Where(p => p.CategoriaId == categoriaId && p.Stock + delta >= 0)
            .ToList();
        foreach (var p in productos) p.Stock += delta;
        return db.SaveChanges();
    }

    public int EliminarPorPrefijoSku(string prefijo)
    {
        using var db = Crear();
        return db.Productos.Where(p => p.Sku.StartsWith(prefijo)).ExecuteDelete();
    }
}
