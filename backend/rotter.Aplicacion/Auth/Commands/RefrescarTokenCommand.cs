using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record RefrescarTokenCommand(string RefreshToken) : IRequest<RespuestaDto<AuthResponseDto>>;

public class RefrescarTokenHandler : IRequestHandler<RefrescarTokenCommand, RespuestaDto<AuthResponseDto>>
{
    private readonly IUsuarioRepositorio   _usuarios;
    private readonly IJwtServicio          _jwt;
    private readonly IEmisorSesionServicio _emisor;

    public RefrescarTokenHandler(IUsuarioRepositorio usuarios, IJwtServicio jwt, IEmisorSesionServicio emisor)
    { _usuarios = usuarios; _jwt = jwt; _emisor = emisor; }

    public async Task<RespuestaDto<AuthResponseDto>> Handle(RefrescarTokenCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.RefreshToken))
            return RespuestaDto<AuthResponseDto>.Fallo("Refresh token requerido.", 401);

        var hash    = _jwt.HashRefreshToken(req.RefreshToken);
        var usuario = await _usuarios.ObtenerPorRefreshTokenHashAsync(hash);

        if (usuario is null || usuario.RefreshTokenExpira is null || usuario.RefreshTokenExpira < DateTime.Now)
            return RespuestaDto<AuthResponseDto>.Fallo("Refresh token inválido o expirado.", 401);

        if (!usuario.Activo) return RespuestaDto<AuthResponseDto>.Fallo("Usuario desactivado.", 403);

        var auth = await _emisor.EmitirAsync(usuario);
        return RespuestaDto<AuthResponseDto>.Ok(auth);
    }
}
