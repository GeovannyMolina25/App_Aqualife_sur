using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface IProductoRepositorio
{
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<List<Producto>> ObtenerTodosAsync(int pagina, int tamano, bool soloActivos = true, string? busqueda = null);
    Task<List<Producto>> ObtenerPromocionesAsync();
    Task<int> TotalAsync(bool soloActivos = true, string? busqueda = null);
    Task<Producto> CrearAsync(Producto producto);
    Task<Producto> ActualizarAsync(Producto producto);
    Task ActualizarStockAsync(int productoId, int cantidad);
}
