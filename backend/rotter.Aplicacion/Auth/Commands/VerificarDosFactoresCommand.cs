using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record VerificarDosFactoresCommand(string TokenDesafio, string Codigo) : IRequest<RespuestaDto<AuthResponseDto>>;

public class VerificarDosFactoresHandler : IRequestHandler<VerificarDosFactoresCommand, RespuestaDto<AuthResponseDto>>
{
    private readonly IUsuarioRepositorio   _usuarios;
    private readonly IJwtServicio          _jwt;
    private readonly ITotpServicio         _totp;
    private readonly ICifradoServicio      _cifrado;
    private readonly IEmisorSesionServicio _emisor;
    private readonly IAuditoriaServicio    _auditoria;

    public VerificarDosFactoresHandler(IUsuarioRepositorio usuarios, IJwtServicio jwt, ITotpServicio totp,
        ICifradoServicio cifrado, IEmisorSesionServicio emisor, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _jwt = jwt; _totp = totp; _cifrado = cifrado; _emisor = emisor; _auditoria = auditoria; }

    public async Task<RespuestaDto<AuthResponseDto>> Handle(VerificarDosFactoresCommand req, CancellationToken ct)
    {
        var usuarioId = _jwt.ValidarTokenDesafio2FA(req.TokenDesafio);
        if (usuarioId is null)
            return RespuestaDto<AuthResponseDto>.Fallo("Token de desafío inválido o expirado.", 401);

        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId.Value);
        if (usuario is null || !usuario.TwoFactorHabilitado || string.IsNullOrEmpty(usuario.TwoFactorSecretCifrado))
            return RespuestaDto<AuthResponseDto>.Fallo("Solicitud inválida.", 400);

        if (!usuario.Activo) return RespuestaDto<AuthResponseDto>.Fallo("Usuario desactivado.", 403);

        var secreto = _cifrado.Descifrar(usuario.TwoFactorSecretCifrado);
        if (!_totp.VerificarCodigo(secreto, req.Codigo))
        {
            await _auditoria.RegistrarAsync("2FA_FALLIDO", "Usuarios", usuario.Id.ToString(), exitoso: false, mensajeError: "Código inválido");
            return RespuestaDto<AuthResponseDto>.Fallo("Código inválido.", 401);
        }

        var auth = await _emisor.EmitirAsync(usuario);
        await _auditoria.RegistrarAsync("LOGIN", "Usuarios", usuario.Id.ToString());

        return RespuestaDto<AuthResponseDto>.Ok(auth);
    }
}
