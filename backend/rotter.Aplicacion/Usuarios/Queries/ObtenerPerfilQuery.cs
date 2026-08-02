using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Usuarios.Queries;

public record ObtenerPerfilQuery(int UsuarioId) : IRequest<RespuestaDto<UsuarioDto>>;

public class ObtenerPerfilHandler : IRequestHandler<ObtenerPerfilQuery, RespuestaDto<UsuarioDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    public ObtenerPerfilHandler(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<RespuestaDto<UsuarioDto>> Handle(ObtenerPerfilQuery req, CancellationToken ct)
    {
        var u = await _usuarios.ObtenerPorIdAsync(req.UsuarioId);
        if (u is null) return RespuestaDto<UsuarioDto>.Fallo("Usuario no encontrado.", 404);

        return RespuestaDto<UsuarioDto>.Ok(new UsuarioDto(u.Id, u.Nombre, u.Apellido, u.Email,
            u.FechaNacimiento, u.Sexo, u.Direccion, u.Telefono, u.Rol.Nombre, u.Activo, u.FechaCreacion));
    }
}
