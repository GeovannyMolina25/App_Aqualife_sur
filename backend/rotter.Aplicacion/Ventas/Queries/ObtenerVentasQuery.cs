using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Aplicacion.Ventas.Commands;

namespace rotter.Aplicacion.Ventas.Queries;

public record ObtenerVentasQuery(int Pagina = 1, int Tamano = 20, DateTime? Desde = null, DateTime? Hasta = null)
    : IRequest<RespuestaDto<PagedResult<VentaDto>>>;

public class ObtenerVentasHandler : IRequestHandler<ObtenerVentasQuery, RespuestaDto<PagedResult<VentaDto>>>
{
    private readonly IVentaRepositorio _ventas;
    public ObtenerVentasHandler(IVentaRepositorio ventas) => _ventas = ventas;

    public async Task<RespuestaDto<PagedResult<VentaDto>>> Handle(ObtenerVentasQuery req, CancellationToken ct)
    {
        var lista   = await _ventas.ObtenerTodasAsync(req.Pagina, req.Tamano, req.Desde, req.Hasta);
        var dtos    = lista.Select(RegistrarVentaHandler.Mapear).ToList();
        var paginas = (int)Math.Ceiling(dtos.Count / (double)req.Tamano);
        return RespuestaDto<PagedResult<VentaDto>>.Ok(new(dtos, dtos.Count, req.Pagina, req.Tamano, paginas));
    }
}
