using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Auth.Commands;

public record CerrarSesionCommand(int UsuarioId) : IRequest<RespuestaDto<bool>>;

public class CerrarSesionHandler : IRequestHandler<CerrarSesionCommand, RespuestaDto<bool>>
{
    private readonly IUsuarioRepositorio _usuarios;
    public CerrarSesionHandler(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<RespuestaDto<bool>> Handle(CerrarSesionCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null) return RespuestaDto<bool>.Fallo("Usuario no encontrado.", 404);

        usuario.RefreshTokenHash   = null;
        usuario.RefreshTokenExpira = null;
        await _usuarios.ActualizarAsync(usuario);

        return RespuestaDto<bool>.Ok(true, "Sesión cerrada.");
    }
}
