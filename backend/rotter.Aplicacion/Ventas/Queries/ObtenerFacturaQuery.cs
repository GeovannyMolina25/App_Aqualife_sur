using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Ventas.Queries;

public record ObtenerFacturaQuery(int VentaId)
    : IRequest<RespuestaDto<FacturaDto>>;

public class ObtenerFacturaHandler
    : IRequestHandler<
        ObtenerFacturaQuery,
        RespuestaDto<FacturaDto>>
{
    private readonly IVentaRepositorio _ventas;

    public ObtenerFacturaHandler(
        IVentaRepositorio ventas)
    {
        _ventas = ventas;
    }

    public async Task<RespuestaDto<FacturaDto>> Handle(
        ObtenerFacturaQuery req,
        CancellationToken ct)
    {
        var factura =
            await _ventas.ObtenerFacturaPorIdAsync(
                req.VentaId
            );

        if (factura == null)
        {
            return RespuestaDto<FacturaDto>.Fallo(
                "Factura no encontrada",
                404
            );
        }
        return RespuestaDto<FacturaDto>.Ok(
            factura
        );
    }
}