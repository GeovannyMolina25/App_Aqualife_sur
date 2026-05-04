using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Reportes.Queries;

public record GenerarExcelQuery(DateTime Desde, DateTime Hasta) : IRequest<RespuestaDto<byte[]>>;

public class GenerarExcelHandler : IRequestHandler<GenerarExcelQuery, RespuestaDto<byte[]>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IReporteServicio  _reportes;

    public GenerarExcelHandler(IVentaRepositorio ventas, IReporteServicio reportes)
    { _ventas = ventas; _reportes = reportes; }

    public async Task<RespuestaDto<byte[]>> Handle(GenerarExcelQuery req, CancellationToken ct)
    {
        var ventas   = await _ventas.ObtenerTodasAsync(1, 10000, req.Desde, req.Hasta);
        var detalles = ventas.SelectMany(v => v.Detalles).ToList();
        return RespuestaDto<byte[]>.Ok(_reportes.GenerarExcelProductosVendidos(detalles));
    }
}
