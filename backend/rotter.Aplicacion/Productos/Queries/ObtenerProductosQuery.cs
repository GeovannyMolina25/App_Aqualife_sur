using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Productos.Queries;

public record ObtenerProductosQuery(int Pagina = 1, int Tamano = 20, bool SoloActivos = true, string? Busqueda = null,
    int? CategoriaId = null, string? Tipo = null)
    : IRequest<RespuestaDto<PagedResult<ProductoDto>>>;

public class ObtenerProductosHandler : IRequestHandler<ObtenerProductosQuery, RespuestaDto<PagedResult<ProductoDto>>>
{
    private readonly IProductoRepositorio _productos;
    public ObtenerProductosHandler(IProductoRepositorio productos) => _productos = productos;

    public async Task<RespuestaDto<PagedResult<ProductoDto>>> Handle(ObtenerProductosQuery req, CancellationToken ct)
    {
        var lista   = await _productos.ObtenerTodosAsync(req.Pagina, req.Tamano, req.SoloActivos, req.Busqueda, req.CategoriaId, req.Tipo);
        var total   = await _productos.TotalAsync(req.SoloActivos, req.Busqueda, req.CategoriaId, req.Tipo);
        var paginas = (int)Math.Ceiling(total / (double)req.Tamano);
        var dtos    = lista.Select(p => new ProductoDto(p.Id, p.Nombre, p.Descripcion, p.Caracteristicas,
            p.Precio, p.Stock, p.CategoriaId, p.Categoria.Nombre, p.Categoria.Tipo, p.ImagenUrl, p.EsPromocion, p.PrecioPromocion,
            p.FechaInicioPromocion, p.FechaFinPromocion, p.Activo)).ToList();

        return RespuestaDto<PagedResult<ProductoDto>>.Ok(new(dtos, total, req.Pagina, req.Tamano, paginas));
    }
}
