using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Auth;

public class EmisorSesionServicio : IEmisorSesionServicio
{
    private static readonly TimeSpan VigenciaRefreshToken = TimeSpan.FromDays(7);

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IJwtServicio        _jwt;

    public EmisorSesionServicio(IUsuarioRepositorio usuarios, IJwtServicio jwt)
    { _usuarios = usuarios; _jwt = jwt; }

    public async Task<AuthResponseDto> EmitirAsync(Usuario usuario)
    {
        var refreshToken = _jwt.GenerarRefreshToken();
        usuario.UltimoAcceso       = DateTime.Now;
        usuario.RefreshTokenHash   = _jwt.HashRefreshToken(refreshToken);
        usuario.RefreshTokenExpira = DateTime.Now.Add(VigenciaRefreshToken);
        await _usuarios.ActualizarAsync(usuario);

        return new AuthResponseDto(
            _jwt.GenerarToken(usuario),
            refreshToken,
            new UsuarioAuthDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Rol.Nombre),
            DateTime.Now.AddMinutes(_jwt.ObtenerMinutosExpiracion()),
            usuario.RequiereCambioPassword
        );
    }
}
