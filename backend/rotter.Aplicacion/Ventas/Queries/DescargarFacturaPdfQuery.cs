using MediatR;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Ventas.Queries;

public record DescargarFacturaPdfQuery(int VentaId)
    : IRequest<byte[]>;

public class DescargarFacturaPdfHandler
    : IRequestHandler<DescargarFacturaPdfQuery, byte[]>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IFacturaPdfService _pdf;

    public DescargarFacturaPdfHandler(
        IVentaRepositorio ventas,
        IFacturaPdfService pdf)
    {
        _ventas = ventas;
        _pdf = pdf;
    }
    public async Task<byte[]> Handle(
        DescargarFacturaPdfQuery request,
        CancellationToken cancellationToken)
    {
        var factura =
            await _ventas.ObtenerFacturaPorIdAsync(
                request.VentaId);

        if (factura == null)
            throw new Exception(
                "Factura no encontrada");

        return _pdf.GenerarFactura(
            factura);
    }
}