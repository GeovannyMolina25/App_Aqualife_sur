using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Ventas.Queries;

public record ObtenerMetricasQuery : IRequest<RespuestaDto<MetricasDto>>;

public class ObtenerMetricasHandler : IRequestHandler<ObtenerMetricasQuery, RespuestaDto<MetricasDto>>
{
    private readonly IVentaRepositorio _ventas;
    public ObtenerMetricasHandler(IVentaRepositorio ventas) => _ventas = ventas;

    public async Task<RespuestaDto<MetricasDto>> Handle(ObtenerMetricasQuery req, CancellationToken ct)
    {
        var data      = await _ventas.ObtenerMetricasAsync();
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var top       = await _ventas.ObtenerTopColaboradoresAsync(inicioMes, DateTime.UtcNow);

        return RespuestaDto<MetricasDto>.Ok(new MetricasDto(
            data.TotalVentasHoy, data.TotalVentasMes, data.VentasHoy, data.ProductosActivos,
            top.Select(t => new TopColaboradorDto(t.Nombre, t.TotalVentas, t.MontoTotal)).ToList()));
    }
}
