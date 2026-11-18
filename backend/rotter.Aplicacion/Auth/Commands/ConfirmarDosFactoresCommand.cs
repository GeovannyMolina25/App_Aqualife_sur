using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record ConfirmarDosFactoresCommand(int UsuarioId, string Codigo) : IRequest<RespuestaDto<bool>>;

public class ConfirmarDosFactoresHandler : IRequestHandler<ConfirmarDosFactoresCommand, RespuestaDto<bool>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly ITotpServicio       _totp;
    private readonly ICifradoServicio    _cifrado;
    private readonly IAuditoriaServicio  _auditoria;
    private readonly IEmailServicio      _email;

    public ConfirmarDosFactoresHandler(IUsuarioRepositorio usuarios, ITotpServicio totp, ICifradoServicio cifrado,
        IAuditoriaServicio auditoria, IEmailServicio email)
    { _usuarios = usuarios; _totp = totp; _cifrado = cifrado; _auditoria = auditoria; _email = email; }

    public async Task<RespuestaDto<bool>> Handle(ConfirmarDosFactoresCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null || string.IsNullOrEmpty(usuario.TwoFactorSecretCifrado))
            return RespuestaDto<bool>.Fallo("Primero debes iniciar la configuración de dos pasos.", 400);

        var secreto = _cifrado.Descifrar(usuario.TwoFactorSecretCifrado);
        if (!_totp.VerificarCodigo(secreto, req.Codigo))
            return RespuestaDto<bool>.Fallo("Código inválido.", 401);

        usuario.TwoFactorHabilitado = true;
        await _usuarios.ActualizarAsync(usuario);
        await _auditoria.RegistrarAsync("2FA_ACTIVADO", "Usuarios", usuario.Id.ToString());

        try
        {
            await _email.EnviarAsync(usuario.Email, "Verificación en dos pasos activada",
                $"<p>Hola {usuario.Nombre},</p><p>Se activó la verificación en dos pasos en tu cuenta. Si no fuiste tú, contacta al administrador de inmediato.</p>");
        }
        catch { /* un fallo al enviar la notificación no debe revertir la activación */ }

        return RespuestaDto<bool>.Ok(true, "Verificación en dos pasos activada.");
    }
}
