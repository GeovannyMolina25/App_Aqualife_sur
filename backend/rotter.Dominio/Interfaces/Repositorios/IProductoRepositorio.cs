using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface IProductoRepositorio
{
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<List<Producto>> ObtenerTodosAsync(int pagina, int tamano, bool soloActivos = true, string? busqueda = null,
        int? categoriaId = null, string? tipo = null);
    Task<List<Producto>> ObtenerPromocionesAsync();
    Task<int> TotalAsync(bool soloActivos = true, string? busqueda = null, int? categoriaId = null, string? tipo = null);
    Task<Producto> CrearAsync(Producto producto);
    Task<Producto> ActualizarAsync(Producto producto);
    Task<bool> ActualizarStockAsync(int productoId, int cantidad);
    /// <summary>True si el producto aparece en alguna venta o cotización (no se puede borrar sin perder historial).</summary>
    Task<bool> TieneHistorialAsync(int productoId);
    Task EliminarAsync(Producto producto);
}
