using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Reportes.Queries;

public record GenerarPdfColaboradorQuery(int ColaboradorId, DateTime Desde, DateTime Hasta) : IRequest<RespuestaDto<byte[]>>;

public class GenerarPdfColaboradorHandler : IRequestHandler<GenerarPdfColaboradorQuery, RespuestaDto<byte[]>>
{
    private readonly IVentaRepositorio   _ventas;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IReporteServicio    _reportes;

    public GenerarPdfColaboradorHandler(IVentaRepositorio ventas, IUsuarioRepositorio usuarios, IReporteServicio reportes)
    { _ventas = ventas; _usuarios = usuarios; _reportes = reportes; }

    public async Task<RespuestaDto<byte[]>> Handle(GenerarPdfColaboradorQuery req, CancellationToken ct)
    {
        var colaborador = await _usuarios.ObtenerPorIdAsync(req.ColaboradorId);
        if (colaborador is null) return RespuestaDto<byte[]>.Fallo("Colaborador no encontrado.", 404);

        var ventas = await _ventas.ObtenerPorColaboradorYPeriodoAsync(req.ColaboradorId, req.Desde, req.Hasta);
        var pdf    = _reportes.GenerarPdfVentasColaborador(ventas, $"{colaborador.Nombre} {colaborador.Apellido}", req.Desde, req.Hasta);
        return RespuestaDto<byte[]>.Ok(pdf);
    }
}
