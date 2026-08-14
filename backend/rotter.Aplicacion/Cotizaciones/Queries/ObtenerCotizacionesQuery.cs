using MediatR;
using rotter.Aplicacion.Cotizaciones.Commands;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Cotizaciones;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Cotizaciones.Queries;

/// <summary>Listado para el personal (nunca para clientes): les permite ver quién pidió un
/// servicio para poder llamarlo, y filtrar por estado de gestión.</summary>
public record ObtenerCotizacionesQuery(int Pagina = 1, int Tamano = 20, string? Estado = null, string? Busqueda = null)
    : IRequest<RespuestaDto<PagedResult<CotizacionServicioDto>>>;

public class ObtenerCotizacionesHandler : IRequestHandler<ObtenerCotizacionesQuery, RespuestaDto<PagedResult<CotizacionServicioDto>>>
{
    private readonly ICotizacionServicioRepositorio _cotizaciones;
    public ObtenerCotizacionesHandler(ICotizacionServicioRepositorio cotizaciones) => _cotizaciones = cotizaciones;

    public async Task<RespuestaDto<PagedResult<CotizacionServicioDto>>> Handle(ObtenerCotizacionesQuery req, CancellationToken ct)
    {
        var lista = await _cotizaciones.ObtenerTodasAsync(req.Pagina, req.Tamano, req.Estado, req.Busqueda);
        var total = await _cotizaciones.TotalAsync(req.Estado, req.Busqueda);
        var paginas = (int)Math.Ceiling(total / (double)req.Tamano);
        var dtos = lista.Select(c => SolicitarCotizacionHandler.Mapear(c, c.Producto.Nombre)).ToList();

        return RespuestaDto<PagedResult<CotizacionServicioDto>>.Ok(new(dtos, total, req.Pagina, req.Tamano, paginas));
    }
}
