using System;
using System.Collections.Generic;

namespace TechStore.EfDbFirst.Models;

public partial class Cliente
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
