using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Usuarios.Commands;

public record CambiarRolCommand(int UsuarioId, int NuevoRolId) : IRequest<RespuestaDto<bool>>;

public class CambiarRolHandler : IRequestHandler<CambiarRolCommand, RespuestaDto<bool>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio  _auditoria;

    public CambiarRolHandler(IUsuarioRepositorio usuarios, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(CambiarRolCommand req, CancellationToken ct)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (usuario is null) return RespuestaDto<bool>.Fallo("Usuario no encontrado.", 404);

        var rolAnterior = usuario.RolId;
        await _usuarios.CambiarRolAsync(req.UsuarioId, req.NuevoRolId);
        await _auditoria.RegistrarAsync("CAMBIO_ROL", "Usuarios", req.UsuarioId.ToString(),
            datosAnteriores: new { RolId = rolAnterior }, datosNuevos: new { RolId = req.NuevoRolId });

        return RespuestaDto<bool>.Ok(true, "Rol actualizado correctamente.");
    }
}
