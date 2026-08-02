using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Ventas.Commands;

public record ActualizarEstadoPagoCommand(int VentaId, string NuevoEstadoPago) : IRequest<RespuestaDto<bool>>;

public class ActualizarEstadoPagoHandler : IRequestHandler<ActualizarEstadoPagoCommand, RespuestaDto<bool>>
{
    private static readonly HashSet<string> Validos = new() { EstadosPago.Verificado, EstadosPago.Rechazado, EstadosPago.PendienteVerificacion };

    private readonly IVentaRepositorio _ventas;
    private readonly IAuditoriaServicio _auditoria;

    public ActualizarEstadoPagoHandler(IVentaRepositorio ventas, IAuditoriaServicio auditoria)
    { _ventas = ventas; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(ActualizarEstadoPagoCommand req, CancellationToken ct)
    {
        if (!Validos.Contains(req.NuevoEstadoPago))
            return RespuestaDto<bool>.Fallo("Estado de pago no válido.", 400);

        var ok = await _ventas.ActualizarEstadoPagoAsync(req.VentaId, req.NuevoEstadoPago);
        if (!ok) return RespuestaDto<bool>.Fallo("Pedido no encontrado.", 404);

        await _auditoria.RegistrarAsync("ACTUALIZAR_ESTADO_PAGO", "Ventas", req.VentaId.ToString(),
            datosNuevos: new { req.NuevoEstadoPago });

        return RespuestaDto<bool>.Ok(true, "Estado de pago actualizado.");
    }
}
