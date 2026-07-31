using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Usuarios.Queries;

public record ObtenerUsuariosQuery(int Pagina = 1, int Tamano = 20, string? Busqueda = null) : IRequest<RespuestaDto<PagedResult<UsuarioDto>>>;

public class ObtenerUsuariosHandler : IRequestHandler<ObtenerUsuariosQuery, RespuestaDto<PagedResult<UsuarioDto>>>
{
    private readonly IUsuarioRepositorio _usuarios;
    public ObtenerUsuariosHandler(IUsuarioRepositorio usuarios) => _usuarios = usuarios;

    public async Task<RespuestaDto<PagedResult<UsuarioDto>>> Handle(ObtenerUsuariosQuery req, CancellationToken ct)
    {
        var lista   = await _usuarios.ObtenerTodosAsync(req.Pagina, req.Tamano, req.Busqueda);
        var total   = await _usuarios.TotalAsync(req.Busqueda);
        var paginas = (int)Math.Ceiling(total / (double)req.Tamano);
        var dtos    = lista.Select(u => new UsuarioDto(u.Id, u.Nombre, u.Apellido, u.Email,
            u.FechaNacimiento, u.Sexo, u.Direccion, u.Telefono, u.Rol.Nombre, u.Activo, u.FechaCreacion)).ToList();

        return RespuestaDto<PagedResult<UsuarioDto>>.Ok(new(dtos, total, req.Pagina, req.Tamano, paginas));
    }
}
