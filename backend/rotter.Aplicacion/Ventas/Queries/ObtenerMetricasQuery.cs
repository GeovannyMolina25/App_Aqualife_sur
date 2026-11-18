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
        var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var top       = await _ventas.ObtenerTopColaboradoresAsync(inicioMes, DateTime.Now);
        var productos = await _ventas.ObtenerTopProductosAsync(6);
        var servicios = await _ventas.ObtenerTopProductosAsync(6, servicios: true);
        var clientes  = await _ventas.ObtenerTopClientesAsync(6);

        return RespuestaDto<MetricasDto>.Ok(new MetricasDto(
            data.TotalVentasHoy, data.TotalVentasMes, data.VentasHoy, data.ProductosActivos,
            top.Select(t => new TopColaboradorDto(t.Nombre, t.TotalVentas, t.MontoTotal)).ToList(),
            productos.Select(p => new TopProductoDto(p.Nombre, p.Unidades, p.MontoTotal)).ToList(),
            servicios.Select(s => new TopProductoDto(s.Nombre, s.Unidades, s.MontoTotal)).ToList(),
            clientes.Select(c => new TopClienteDto(c.Nombre, c.Email, c.Compras, c.Unidades, c.TotalGastado)).ToList()));
    }
}
