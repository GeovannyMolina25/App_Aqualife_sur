using MediatR;
using rotter.Dominio.DTOs.Auditoria;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Auditoria.Queries;

public record ObtenerAuditoriaQuery(int Pagina = 1, int Tamano = 20, string? Accion = null) : IRequest<RespuestaDto<PagedResult<AuditoriaDto>>>;

public class ObtenerAuditoriaHandler : IRequestHandler<ObtenerAuditoriaQuery, RespuestaDto<PagedResult<AuditoriaDto>>>
{
    private readonly IAuditoriaRepositorio _auditoria;
    public ObtenerAuditoriaHandler(IAuditoriaRepositorio auditoria) => _auditoria = auditoria;

    public async Task<RespuestaDto<PagedResult<AuditoriaDto>>> Handle(ObtenerAuditoriaQuery req, CancellationToken ct)
    {
        var lista   = await _auditoria.ObtenerAsync(req.Pagina, req.Tamano, req.Accion);
        var total   = await _auditoria.TotalAsync(req.Accion);
        var paginas = (int)Math.Ceiling(total / (double)req.Tamano);
        var dtos    = lista.Select(a => new AuditoriaDto(
            a.Id, a.UsuarioId, a.UsuarioEmail, a.Accion, a.Entidad, a.EntidadId,
            a.IpAddress, a.FechaAccion, a.Exitoso, a.MensajeError)).ToList();

        return RespuestaDto<PagedResult<AuditoriaDto>>.Ok(new(dtos, total, req.Pagina, req.Tamano, paginas));
    }
}
