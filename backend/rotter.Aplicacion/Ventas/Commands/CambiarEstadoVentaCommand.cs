using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Ventas.Commands;

public record CambiarEstadoVentaCommand(int VentaId, string NuevoEstado) : IRequest<RespuestaDto<bool>>;

public class CambiarEstadoVentaHandler : IRequestHandler<CambiarEstadoVentaCommand, RespuestaDto<bool>>
{
    private static readonly string[] EstadosValidos = { "Pendiente", "Completada", "Ingresada", "Anulada" };

    private readonly IVentaRepositorio _ventas;
    private readonly IAuditoriaServicio _auditoria;

    public CambiarEstadoVentaHandler(IVentaRepositorio ventas, IAuditoriaServicio auditoria)
    { _ventas = ventas; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(CambiarEstadoVentaCommand req, CancellationToken ct)
    {
        if (!EstadosValidos.Contains(req.NuevoEstado))
            return RespuestaDto<bool>.Fallo("Estado no válido. Use: Pendiente, Completada, Ingresada o Anulada.", 400);

        var venta = await _ventas.ObtenerPorIdAsync(req.VentaId);
        if (venta is null) return RespuestaDto<bool>.Fallo("Venta no encontrada.", 404);

        var estadoAnterior = venta.Estado;
        await _ventas.ActualizarEstadoAsync(req.VentaId, req.NuevoEstado);
        await _auditoria.RegistrarAsync("CAMBIO_ESTADO_VENTA", "Ventas", req.VentaId.ToString(),
            datosAnteriores: new { Estado = estadoAnterior }, datosNuevos: new { Estado = req.NuevoEstado });

        return RespuestaDto<bool>.Ok(true, "Estado de la venta actualizado correctamente.");
    }
}
