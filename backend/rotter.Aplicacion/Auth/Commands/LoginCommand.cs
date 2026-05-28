using BCrypt.Net;
using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record LoginCommand(LoginDto Dto) : IRequest<RespuestaDto<AuthResponseDto>>;

public class LoginHandler : IRequestHandler<LoginCommand, RespuestaDto<AuthResponseDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IJwtServicio        _jwt;
    private readonly IAuditoriaServicio  _auditoria;

    public LoginHandler(IUsuarioRepositorio usuarios, IJwtServicio jwt, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _jwt = jwt; _auditoria = auditoria; }

    public async Task<RespuestaDto<AuthResponseDto>> Handle(LoginCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorEmailAsync(req.Dto.Email);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Dto.Password, usuario.PasswordHash))
        {
            await _auditoria.RegistrarAsync("LOGIN_FALLIDO", "Usuarios", exitoso: false, mensajeError: "Credenciales inválidas");
            return RespuestaDto<AuthResponseDto>.Fallo("Credenciales inválidas.", 401);
        }

        if (!usuario.Activo) return RespuestaDto<AuthResponseDto>.Fallo("Usuario desactivado.", 403);

        var debeCambiarPassword = req.Dto.Password.StartsWith("Rotter");
        usuario.UltimoAcceso = DateTime.Now;
        await _usuarios.ActualizarAsync(usuario);
        await _auditoria.RegistrarAsync("LOGIN", "Usuarios", usuario.Id.ToString());

        return RespuestaDto<AuthResponseDto>
            .Ok(new AuthResponseDto(
            _jwt.GenerarToken(usuario), 
            _jwt.GenerarRefreshToken(),

            new UsuarioAuthDto(
                usuario.Id, 
                usuario.Nombre, 
                usuario.Apellido, 
                usuario.Email, 
                usuario.Rol.Nombre
                ),
            DateTime.Now.AddHours(24),
            debeCambiarPassword
            ));
    }
}
