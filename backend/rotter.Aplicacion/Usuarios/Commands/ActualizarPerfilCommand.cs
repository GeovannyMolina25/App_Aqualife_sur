using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Usuarios.Commands;

public record ActualizarPerfilCommand(int UsuarioId, ActualizarPerfilDto Dto) : IRequest<RespuestaDto<UsuarioDto>>;

public class ActualizarPerfilHandler : IRequestHandler<ActualizarPerfilCommand, RespuestaDto<UsuarioDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    public ActualizarPerfilHandler(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<RespuestaDto<UsuarioDto>> Handle(ActualizarPerfilCommand req, CancellationToken ct)
    {
        var u = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (u is null) return RespuestaDto<UsuarioDto>.Fallo("Usuario no encontrado.", 404);

        u.Nombre = req.Dto.Nombre;
        u.Apellido = req.Dto.Apellido;
        u.Direccion = req.Dto.Direccion;
        u.Telefono = req.Dto.Telefono;

        await _usuarios.ActualizarAsync(u);

        return RespuestaDto<UsuarioDto>.Ok(new UsuarioDto(u.Id, u.Nombre, u.Apellido, u.Email,
            u.FechaNacimiento, u.Sexo, u.Direccion, u.Telefono, u.Rol.Nombre, u.Activo, u.FechaCreacion,
            u.PremioBienvenida, u.RecargasParaSeptimo, u.PremioBienvenidaEntregado),
            "Perfil actualizado.");
    }
}
