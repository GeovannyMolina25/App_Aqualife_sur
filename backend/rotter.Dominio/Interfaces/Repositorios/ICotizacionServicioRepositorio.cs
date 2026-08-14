using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface ICotizacionServicioRepositorio
{
    Task<CotizacionServicio> CrearAsync(CotizacionServicio cotizacion);
    Task<CotizacionServicio?> ObtenerPorIdAsync(int id);
    Task<List<CotizacionServicio>> ObtenerTodasAsync(int pagina, int tamano, string? estado = null, string? busqueda = null);
    Task<int> TotalAsync(string? estado = null, string? busqueda = null);
    Task<CotizacionServicio> ActualizarAsync(CotizacionServicio cotizacion);
}
