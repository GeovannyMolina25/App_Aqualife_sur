using MediatR;
using rotter.Aplicacion.Ventas.Commands;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Caja.Queries;

public record ObtenerVentasPendientesQuery : IRequest<RespuestaDto<List<VentaDto>>>;

public class ObtenerVentasPendientesHandler : IRequestHandler<ObtenerVentasPendientesQuery, RespuestaDto<List<VentaDto>>>
{
    private readonly ICajaRepositorio _caja;
    public ObtenerVentasPendientesHandler(ICajaRepositorio caja) => _caja = caja;

    public async Task<RespuestaDto<List<VentaDto>>> Handle(ObtenerVentasPendientesQuery req, CancellationToken ct)
    {
        var ventas = await _caja.ObtenerVentasPendientesDeIngresoAsync();
        return RespuestaDto<List<VentaDto>>.Ok(ventas.Select(RegistrarVentaHandler.Mapear).ToList());
    }
}
