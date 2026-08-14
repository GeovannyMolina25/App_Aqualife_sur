using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Usuarios.Queries;

public record ObtenerEstadoPromocionQuery(int UsuarioId) : IRequest<RespuestaDto<EstadoPromocionDto>>;

public class ObtenerEstadoPromocionHandler : IRequestHandler<ObtenerEstadoPromocionQuery, RespuestaDto<EstadoPromocionDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    public ObtenerEstadoPromocionHandler(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<RespuestaDto<EstadoPromocionDto>> Handle(ObtenerEstadoPromocionQuery req, CancellationToken ct)
    {
        var u = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (u is null) return RespuestaDto<EstadoPromocionDto>.Fallo("Usuario no encontrado.", 404);

        return RespuestaDto<EstadoPromocionDto>.Ok(new EstadoPromocionDto(
            u.PremioBienvenida is null,
            u.PremioBienvenida,
            u.RecargasParaSeptimo,
            u.PremioBienvenidaEntregado));
    }
}
