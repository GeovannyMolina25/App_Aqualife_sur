using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Servicios;

public interface IReporteServicio
{
    byte[] GenerarPdfVentasMensuales(List<Venta> ventas, int anio, int mes);
    byte[] GenerarPdfVentasColaborador(List<Venta> ventas, string nombreColaborador, DateTime desde, DateTime hasta);
    byte[] GenerarExcelProductosVendidos(List<DetalleVenta> detalles);
}
