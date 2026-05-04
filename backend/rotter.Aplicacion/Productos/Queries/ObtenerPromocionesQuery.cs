using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Productos.Queries;

public record ObtenerPromocionesQuery : IRequest<RespuestaDto<List<ProductoDto>>>;

public class ObtenerPromocionesHandler : IRequestHandler<ObtenerPromocionesQuery, RespuestaDto<List<ProductoDto>>>
{
    private readonly IProductoRepositorio _productos;
    public ObtenerPromocionesHandler(IProductoRepositorio productos) => _productos = productos;

    public async Task<RespuestaDto<List<ProductoDto>>> Handle(ObtenerPromocionesQuery req, CancellationToken ct)
    {
        var lista = await _productos.ObtenerPromocionesAsync();
        var dtos  = lista.Select(p => new ProductoDto(p.Id, p.Nombre, p.Descripcion, p.Caracteristicas,
            p.Precio, p.Stock, p.Categoria.Nombre, p.ImagenUrl, p.EsPromocion, p.PrecioPromocion,
            p.FechaInicioPromocion, p.FechaFinPromocion, p.Activo)).ToList();

        return RespuestaDto<List<ProductoDto>>.Ok(dtos);
    }
}
