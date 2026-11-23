using System;
using System.Collections.Generic;

namespace TechStore.EfDbFirst.Models;

public partial class Producto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public int CategoriaId { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public DateTime FechaCreacion { get; set; }

    public bool Activo { get; set; }

    public virtual Categoria Categoria { get; set; } = null!;

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
}
