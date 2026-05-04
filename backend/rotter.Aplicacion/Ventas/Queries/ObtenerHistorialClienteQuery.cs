using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Aplicacion.Ventas.Commands;

namespace rotter.Aplicacion.Ventas.Queries;

public record ObtenerHistorialClienteQuery(int ClienteId, int Pagina = 1, int Tamano = 20)
    : IRequest<RespuestaDto<PagedResult<VentaDto>>>;

public class ObtenerHistorialClienteHandler : IRequestHandler<ObtenerHistorialClienteQuery, RespuestaDto<PagedResult<VentaDto>>>
{
    private readonly IVentaRepositorio _ventas;
    public ObtenerHistorialClienteHandler(IVentaRepositorio ventas) => _ventas = ventas;

    public async Task<RespuestaDto<PagedResult<VentaDto>>> Handle(ObtenerHistorialClienteQuery req, CancellationToken ct)
    {
        var lista   = await _ventas.ObtenerPorClienteAsync(req.ClienteId, req.Pagina, req.Tamano);
        var dtos    = lista.Select(RegistrarVentaHandler.Mapear).ToList();
        var paginas = (int)Math.Ceiling(dtos.Count / (double)req.Tamano);
        return RespuestaDto<PagedResult<VentaDto>>.Ok(new(dtos, dtos.Count, req.Pagina, req.Tamano, paginas));
    }
}
