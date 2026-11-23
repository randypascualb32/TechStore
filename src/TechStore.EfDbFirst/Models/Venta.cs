using System;
using System.Collections.Generic;

namespace TechStore.EfDbFirst.Models;

public partial class Venta
{
    public int VentaId { get; set; }

    public int ClienteId { get; set; }

    public DateTime Fecha { get; set; }

    public decimal Total { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetalleVenta> DetalleVenta { get; set; } = new List<DetalleVenta>();
}
