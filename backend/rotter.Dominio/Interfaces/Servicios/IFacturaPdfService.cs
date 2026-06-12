using rotter.Dominio.DTOs.Ventas;

namespace rotter.Dominio.Interfaces.Servicios;

public interface IFacturaPdfService
{
    byte[] GenerarFactura(FacturaDto factura);
}