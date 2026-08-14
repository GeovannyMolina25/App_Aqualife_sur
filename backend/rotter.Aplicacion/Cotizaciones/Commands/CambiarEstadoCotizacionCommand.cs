using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Cotizaciones;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Cotizaciones.Commands;

public record CambiarEstadoCotizacionCommand(int Id, string Estado) : IRequest<RespuestaDto<CotizacionServicioDto>>;

public class CambiarEstadoCotizacionHandler : IRequestHandler<CambiarEstadoCotizacionCommand, RespuestaDto<CotizacionServicioDto>>
{
    private static readonly string[] EstadosValidos =
        [EstadosCotizacion.Entrante, EstadosCotizacion.Pendiente, EstadosCotizacion.Realizado, EstadosCotizacion.Anulado];

    private readonly ICotizacionServicioRepositorio _cotizaciones;
    public CambiarEstadoCotizacionHandler(ICotizacionServicioRepositorio cotizaciones) => _cotizaciones = cotizaciones;

    public async Task<RespuestaDto<CotizacionServicioDto>> Handle(CambiarEstadoCotizacionCommand req, CancellationToken ct)
    {
        if (!EstadosValidos.Contains(req.Estado))
            return RespuestaDto<CotizacionServicioDto>.Fallo("Estado inválido.");

        var cotizacion = await _cotizaciones.ObtenerPorIdAsync(req.Id);
        if (cotizacion is null) return RespuestaDto<CotizacionServicioDto>.Fallo("Cotización no encontrada.", 404);

        cotizacion.Estado = req.Estado;
        await _cotizaciones.ActualizarAsync(cotizacion);

        return RespuestaDto<CotizacionServicioDto>.Ok(
            SolicitarCotizacionHandler.Mapear(cotizacion, cotizacion.Producto.Nombre),
            "Estado actualizado.");
    }
}
