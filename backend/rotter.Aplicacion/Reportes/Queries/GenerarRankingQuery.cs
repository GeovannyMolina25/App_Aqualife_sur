using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Reportes;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Reportes.Queries;

public enum FormatoReporte { Pdf, Excel }

/// <summary>Ranking completo de clientes, productos y servicios. Sin fechas = todo el historial.</summary>
public record GenerarRankingQuery(DateTime? Desde, DateTime? Hasta, FormatoReporte Formato) : IRequest<RespuestaDto<byte[]>>;

public class GenerarRankingHandler : IRequestHandler<GenerarRankingQuery, RespuestaDto<byte[]>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IReporteServicio  _reportes;

    public GenerarRankingHandler(IVentaRepositorio ventas, IReporteServicio reportes)
    { _ventas = ventas; _reportes = reportes; }

    public async Task<RespuestaDto<byte[]>> Handle(GenerarRankingQuery req, CancellationToken ct)
    {
        if (req.Desde.HasValue && req.Hasta.HasValue && req.Desde > req.Hasta)
            return RespuestaDto<byte[]>.Fallo("La fecha 'desde' no puede ser mayor que 'hasta'.");

        var datos = new RankingReporteDto(
            req.Desde, req.Hasta,
            await _ventas.ObtenerTopClientesAsync(null, req.Desde, req.Hasta),
            await _ventas.ObtenerTopProductosAsync(null, servicios: false, req.Desde, req.Hasta, incluirSinVentas: true),
            await _ventas.ObtenerTopProductosAsync(null, servicios: true, req.Desde, req.Hasta, incluirSinVentas: true));

        var archivo = req.Formato == FormatoReporte.Pdf
            ? _reportes.GenerarPdfRanking(datos)
            : _reportes.GenerarExcelRanking(datos);

        return RespuestaDto<byte[]>.Ok(archivo);
    }
}
