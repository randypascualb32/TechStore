namespace TechStore.Core;

public class Categoria
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = "";
}

public class Producto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = "";
    public string Sku { get; set; } = "";
    public int CategoriaId { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public DateTime FechaCreacion { get; set; }
    public bool Activo { get; set; } = true;
}

public class Cliente
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
}

public class Venta
{
    public int VentaId { get; set; }
    public int ClienteId { get; set; }
    public DateTime Fecha { get; set; }
    public decimal Total { get; set; }
    public List<DetalleVenta> Detalles { get; set; } = new();
}

public class DetalleVenta
{
    public int DetalleId { get; set; }
    public int VentaId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}

/// <summary>Producto y cantidad que se quiere comprar en una venta.</summary>
public record ItemVenta(int ProductoId, int Cantidad);
