using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Productos.Queries;

public record ObtenerCategoriasQuery(bool SoloActivas = true) : IRequest<RespuestaDto<List<CategoriaDto>>>;

public class ObtenerCategoriasHandler : IRequestHandler<ObtenerCategoriasQuery, RespuestaDto<List<CategoriaDto>>>
{
    private readonly ICategoriaRepositorio _categorias;
    public ObtenerCategoriasHandler(ICategoriaRepositorio categorias) => _categorias = categorias;

    public async Task<RespuestaDto<List<CategoriaDto>>> Handle(ObtenerCategoriasQuery req, CancellationToken ct)
    {
        var lista = await _categorias.ObtenerTodasAsync(req.SoloActivas);
        var dtos = lista.Select(c => new CategoriaDto(c.Id, c.Nombre, c.Descripcion, c.Tipo)).ToList();
        return RespuestaDto<List<CategoriaDto>>.Ok(dtos);
    }
}
