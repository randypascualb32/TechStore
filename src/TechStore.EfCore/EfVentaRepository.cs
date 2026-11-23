using Microsoft.EntityFrameworkCore;
using TechStore.Core;

namespace TechStore.EfCore;

public class EfVentaRepository : IVentaRepository
{
    private readonly string _cs;

    public EfVentaRepository(string connectionString) => _cs = connectionString;

    public int RegistrarVenta(int clienteId, IReadOnlyList<ItemVenta> items)
    {
        using var db = TechStoreContext.Crear(_cs);

        var venta = new Venta { ClienteId = clienteId };
        foreach (var item in items)
        {
            var producto = db.Productos.FirstOrDefault(p => p.ProductoId == item.ProductoId && p.Activo);
            if (producto is null || producto.Stock < item.Cantidad)
                throw new InvalidOperationException(
                    $"Producto {item.ProductoId} inexistente, inactivo o sin stock suficiente.");

            producto.Stock -= item.Cantidad;
            venta.Total += producto.Precio * item.Cantidad;
            venta.Detalles.Add(new DetalleVenta
            {
                ProductoId = producto.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        db.Ventas.Add(venta);
        // SaveChanges() guarda todo (stock, venta y detalles) dentro de UNA transacción implícita:
        // si algo falla, no se guarda nada.
        db.SaveChanges();
        return venta.VentaId;
    }

    public Venta? ObtenerPorId(int ventaId)
    {
        using var db = TechStoreContext.Crear(_cs);
        return db.Ventas
            .Include(v => v.Detalles)
            .AsNoTracking()
            .FirstOrDefault(v => v.VentaId == ventaId);
    }
}
