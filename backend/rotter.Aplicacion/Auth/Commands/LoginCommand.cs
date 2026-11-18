using BCrypt.Net;
using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record LoginCommand(LoginDto Dto) : IRequest<RespuestaDto<LoginResultadoDto>>;

public class LoginHandler : IRequestHandler<LoginCommand, RespuestaDto<LoginResultadoDto>>
{
    private const int MaxIntentosFallidos = 5;
    private static readonly TimeSpan DuracionBloqueo = TimeSpan.FromMinutes(15);

    private readonly IUsuarioRepositorio    _usuarios;
    private readonly IJwtServicio           _jwt;
    private readonly IEmisorSesionServicio  _emisor;
    private readonly IAuditoriaServicio     _auditoria;
    private readonly IEmailServicio         _email;

    public LoginHandler(IUsuarioRepositorio usuarios, IJwtServicio jwt, IEmisorSesionServicio emisor,
        IAuditoriaServicio auditoria, IEmailServicio email)
    { _usuarios = usuarios; _jwt = jwt; _emisor = emisor; _auditoria = auditoria; _email = email; }

    public async Task<RespuestaDto<LoginResultadoDto>> Handle(LoginCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorEmailAsync(req.Dto.Email);

        if (usuario is not null && usuario.BloqueadoHasta > DateTime.Now)
            return RespuestaDto<LoginResultadoDto>.Fallo(
                $"Cuenta bloqueada temporalmente por múltiples intentos fallidos. Intenta de nuevo después de las {usuario.BloqueadoHasta:HH:mm}.", 423);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Dto.Password, usuario.PasswordHash))
        {
            if (usuario is not null)
            {
                usuario.IntentosFallidos++;
                if (usuario.IntentosFallidos >= MaxIntentosFallidos)
                {
                    usuario.BloqueadoHasta = DateTime.Now.Add(DuracionBloqueo);
                    usuario.IntentosFallidos = 0;
                    await NotificarBloqueoAsync(usuario.Email, usuario.Nombre);
                }
                await _usuarios.ActualizarAsync(usuario);
            }
            await _auditoria.RegistrarAsync("LOGIN_FALLIDO", "Usuarios", exitoso: false, mensajeError: "Credenciales inválidas");
            return RespuestaDto<LoginResultadoDto>.Fallo("Credenciales inválidas.", 401);
        }

        if (!usuario.Activo) return RespuestaDto<LoginResultadoDto>.Fallo("Usuario desactivado.", 403);

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta   = null;

        if (usuario.TwoFactorHabilitado)
        {
            await _usuarios.ActualizarAsync(usuario);
            var tokenDesafio = _jwt.GenerarTokenDesafio2FA(usuario.Id);
            return RespuestaDto<LoginResultadoDto>.Ok(
                new LoginResultadoDto(true, tokenDesafio, null),
                "Se requiere el código de verificación."
            );
        }

        var auth = await _emisor.EmitirAsync(usuario);
        await _auditoria.RegistrarAsync("LOGIN", "Usuarios", usuario.Id.ToString());

        return RespuestaDto<LoginResultadoDto>.Ok(new LoginResultadoDto(false, null, auth));
    }

    private async Task NotificarBloqueoAsync(string email, string nombre)
    {
        try
        {
            await _email.EnviarAsync(email, "Alerta de seguridad: cuenta bloqueada temporalmente",
                $"<p>Hola {nombre},</p><p>Tu cuenta fue bloqueada 15 minutos por múltiples intentos fallidos de inicio de sesión. " +
                "Si no fuiste tú, te recomendamos cambiar tu contraseña en cuanto puedas volver a entrar.</p>");
        }
        catch { /* un fallo al enviar el correo de alerta no debe bloquear el flujo de login */ }
    }
}
