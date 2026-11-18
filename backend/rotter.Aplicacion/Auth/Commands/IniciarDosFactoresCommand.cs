using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record IniciarDosFactoresCommand(int UsuarioId) : IRequest<RespuestaDto<DosFactoresSetupDto>>;

public class IniciarDosFactoresHandler : IRequestHandler<IniciarDosFactoresCommand, RespuestaDto<DosFactoresSetupDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly ITotpServicio       _totp;
    private readonly ICifradoServicio    _cifrado;

    public IniciarDosFactoresHandler(IUsuarioRepositorio usuarios, ITotpServicio totp, ICifradoServicio cifrado)
    { _usuarios = usuarios; _totp = totp; _cifrado = cifrado; }

    public async Task<RespuestaDto<DosFactoresSetupDto>> Handle(IniciarDosFactoresCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null) return RespuestaDto<DosFactoresSetupDto>.Fallo("Usuario no encontrado.", 404);

        if (usuario.TwoFactorHabilitado)
            return RespuestaDto<DosFactoresSetupDto>.Fallo("La verificación en dos pasos ya está activa.", 400);

        var secreto = _totp.GenerarSecreto();
        usuario.TwoFactorSecretCifrado = _cifrado.Cifrar(secreto);
        await _usuarios.ActualizarAsync(usuario);

        return RespuestaDto<DosFactoresSetupDto>.Ok(
            new DosFactoresSetupDto(secreto, _totp.ObtenerUriConfiguracion(secreto, usuario.Email)),
            "Escanea el código con tu app autenticadora y confirma con un código para activarlo."
        );
    }
}
