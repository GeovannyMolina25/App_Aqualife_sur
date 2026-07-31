using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface ICajaRepositorio
{
    Task<string> GenerarNumeroIngresoAsync();
    Task<string> GenerarNumeroSalidaAsync();
    Task<IngresoCaja> CrearIngresoAsync(IngresoCaja ingreso, List<int> ventaIds);
    Task<SalidaCaja> CrearSalidaAsync(SalidaCaja salida);
    Task<IngresoCaja?> ObtenerIngresoPorIdAsync(int id);
    Task<SaldoCajaDto> ObtenerSaldoAsync();
    Task<PagedResult<MovimientoCajaDto>> ObtenerHistorialAsync(int pagina, int tamano, string? busqueda = null);
    Task<List<Venta>> ObtenerVentasPendientesDeIngresoAsync();
}
