using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Usuarios.Commands;

public record EntregarPremioBienvenidaCommand(int UsuarioId) : IRequest<RespuestaDto<bool>>;

/// <summary>
/// Lo ejecuta el personal (nunca el cliente) cuando entrega físicamente el premio de
/// bienvenida. Para el 7º botellón gratis exige que ya se hayan cumplido las 7 recargas;
/// una vez entregado no se puede volver a marcar (PremioBienvenidaEntregado ya no permite re-entregar).
/// </summary>
public class EntregarPremioBienvenidaHandler : IRequestHandler<EntregarPremioBienvenidaCommand, RespuestaDto<bool>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio _auditoria;

    public EntregarPremioBienvenidaHandler(IUsuarioRepositorio usuarios, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(EntregarPremioBienvenidaCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null) return RespuestaDto<bool>.Fallo("Usuario no encontrado.", 404);

        if (usuario.PremioBienvenida is null)
            return RespuestaDto<bool>.Fallo("Este usuario todavía no tiene un premio de bienvenida.", 400);

        if (usuario.PremioBienvenidaEntregado)
            return RespuestaDto<bool>.Fallo("Este premio ya fue entregado anteriormente.", 400);

        if (usuario.PremioBienvenida == PremiosBienvenida.SeptimoBotellonGratis && usuario.RecargasParaSeptimo < 7)
            return RespuestaDto<bool>.Fallo($"Aún le faltan recargas ({usuario.RecargasParaSeptimo}/7) para el botellón gratis.", 400);

        usuario.PremioBienvenidaEntregado = true;
        await _usuarios.ActualizarAsync(usuario);

        await _auditoria.RegistrarAsync("ENTREGAR_PREMIO_BIENVENIDA", "Usuarios", usuario.Id.ToString(),
            datosNuevos: new { usuario.PremioBienvenida });

        return RespuestaDto<bool>.Ok(true, "Premio marcado como entregado.");
    }
}
