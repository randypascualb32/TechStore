namespace TechStore.Core;

/// <summary>
/// Contrato que cumplen las DOS implementaciones (ADO.NET y EF Core).
/// Las pruebas de rendimiento usan solo esta interfaz, así se compara lo mismo.
/// </summary>
public interface IProductoRepository
{
    Producto? ObtenerPorId(int id);
    List<Producto> ObtenerTodos(int limite);
    List<Producto> Buscar(int categoriaId, decimal precioMin, decimal precioMax, int limite);
    int Insertar(Producto producto);
    int InsertarVarios(IReadOnlyList<Producto> productos);
    bool Actualizar(Producto producto);
    bool Eliminar(int id);
    int AjustarStockPorCategoria(int categoriaId, int delta);
    int EliminarPorPrefijoSku(string prefijo);
}

public interface IVentaRepository
{
    /// <summary>
    /// Registra una venta completa en una sola transacción: descuenta stock, crea la venta y sus detalles.
    /// Si algo falla (por ejemplo, stock insuficiente) no se guarda nada. Devuelve el Id de la venta.
    /// </summary>
    int RegistrarVenta(int clienteId, IReadOnlyList<ItemVenta> items);
    Venta? ObtenerPorId(int ventaId);
}
