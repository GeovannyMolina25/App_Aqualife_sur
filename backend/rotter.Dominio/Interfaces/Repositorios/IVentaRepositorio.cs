using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface IVentaRepositorio
{
    Task<Venta?> ObtenerPorIdAsync(int id);
    Task<List<Venta>> ObtenerTodasAsync(int pagina, int tamano, DateTime? desde = null, DateTime? hasta = null, string? busqueda = null);
    Task<int> TotalAsync(DateTime? desde = null, DateTime? hasta = null, string? busqueda = null);
    Task<List<Venta>> ObtenerPorClienteAsync(int clienteId, int pagina, int tamano);
    Task<List<Venta>> ObtenerPorColaboradorAsync(int colaboradorId, int pagina, int tamano);
    Task<List<Venta>> ObtenerPorMesAsync(int Anio, int mes);
    Task<List<Venta>> ObtenerPorColaboradorYPeriodoAsync(int colaboradorId, DateTime desde, DateTime hasta);
    Task<Venta> CrearAsync(Venta venta);
    Task<bool> ActualizarEstadoAsync(int ventaId, string nuevoEstado);
    Task<bool> ActualizarComprobanteAsync(int ventaId, string comprobanteUrl, string estadoPago);
    Task<bool> ActualizarEstadoPagoAsync(int ventaId, string estadoPago);
    Task<string> GenerarNumeroVentaAsync();
    Task<MetricasVentaData> ObtenerMetricasAsync();
    Task<FacturaDto?> ObtenerFacturaPorIdAsync(int ventaId);
    Task<List<TopColaboradorData>> ObtenerTopColaboradoresAsync(DateTime desde, DateTime hasta);
    Task<List<TopProductoData>> ObtenerTopProductosAsync(int? cantidad, bool servicios = false,
        DateTime? desde = null, DateTime? hasta = null, bool incluirSinVentas = false);
    Task<List<TopClienteData>> ObtenerTopClientesAsync(int? cantidad, DateTime? desde = null, DateTime? hasta = null);
}

public record MetricasVentaData(decimal TotalVentasHoy, decimal TotalVentasMes, int VentasHoy, int ProductosActivos);
public record TopColaboradorData(string Nombre, int TotalVentas, decimal MontoTotal);
public record TopProductoData(string Nombre, int Unidades, decimal MontoTotal, string Categoria = "");
public record TopClienteData(string Nombre, string Email, int Compras, int Unidades, decimal TotalGastado, DateTime? UltimaCompra = null);
