using MediatR;
using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Caja.Queries;

public record ObtenerHistorialCajaQuery(int Pagina = 1, int Tamano = 20, string? Busqueda = null) : IRequest<RespuestaDto<PagedResult<MovimientoCajaDto>>>;

public class ObtenerHistorialCajaHandler : IRequestHandler<ObtenerHistorialCajaQuery, RespuestaDto<PagedResult<MovimientoCajaDto>>>
{
    private readonly ICajaRepositorio _caja;
    public ObtenerHistorialCajaHandler(ICajaRepositorio caja) => _caja = caja;

    public async Task<RespuestaDto<PagedResult<MovimientoCajaDto>>> Handle(ObtenerHistorialCajaQuery req, CancellationToken ct) =>
        RespuestaDto<PagedResult<MovimientoCajaDto>>.Ok(await _caja.ObtenerHistorialAsync(req.Pagina, req.Tamano, req.Busqueda));
}
