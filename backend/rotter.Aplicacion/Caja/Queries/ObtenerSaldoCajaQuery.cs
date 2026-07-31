using MediatR;
using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Caja.Queries;

public record ObtenerSaldoCajaQuery : IRequest<RespuestaDto<SaldoCajaDto>>;

public class ObtenerSaldoCajaHandler : IRequestHandler<ObtenerSaldoCajaQuery, RespuestaDto<SaldoCajaDto>>
{
    private readonly ICajaRepositorio _caja;
    public ObtenerSaldoCajaHandler(ICajaRepositorio caja) => _caja = caja;

    public async Task<RespuestaDto<SaldoCajaDto>> Handle(ObtenerSaldoCajaQuery req, CancellationToken ct) =>
        RespuestaDto<SaldoCajaDto>.Ok(await _caja.ObtenerSaldoAsync());
}
