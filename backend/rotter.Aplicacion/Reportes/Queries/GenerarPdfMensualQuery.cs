using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Reportes.Queries;

public record GenerarPdfMensualQuery(int Anio, int Mes)
    : IRequest<RespuestaDto<byte[]>>;

public class GenerarPdfMensualHandler : IRequestHandler<GenerarPdfMensualQuery, RespuestaDto<byte[]>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IReporteServicio  _reportes;

    public GenerarPdfMensualHandler(IVentaRepositorio ventas, IReporteServicio reportes)
    { _ventas = ventas; _reportes = reportes; }

    public async Task<RespuestaDto<byte[]>> Handle(GenerarPdfMensualQuery req, CancellationToken ct)
    {
        var ventas = await _ventas.ObtenerPorMesAsync(req.Anio, req.Mes);
        return RespuestaDto<byte[]>.Ok(
            _reportes.GenerarPdfVentasMensuales(
                ventas,
                req.Anio,
                req.Mes
            )
        );
    }
}
