using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Ventas.Commands;

public record SubirComprobanteCommand(
    int VentaId,
    int ClienteId,
    bool EsStaff,
    Stream Contenido,
    string NombreOriginal,
    string ContentType,
    long TamanoBytes
) : IRequest<RespuestaDto<string>>;

public class SubirComprobanteHandler : IRequestHandler<SubirComprobanteCommand, RespuestaDto<string>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IArchivoServicio _archivos;

    public SubirComprobanteHandler(IVentaRepositorio ventas, IArchivoServicio archivos)
    { _ventas = ventas; _archivos = archivos; }

    public async Task<RespuestaDto<string>> Handle(SubirComprobanteCommand req, CancellationToken ct)
    {
        var venta = await _ventas.ObtenerPorIdAsync(req.VentaId);
        if (venta is null) return RespuestaDto<string>.Fallo("Pedido no encontrado.", 404);

        if (!req.EsStaff && venta.ClienteId != req.ClienteId)
            return RespuestaDto<string>.Fallo("No puedes subir un comprobante para este pedido.", 403);

        if (venta.MetodoPago != MetodosPago.TransferenciaProdubanco && venta.MetodoPago != MetodosPago.TransferenciaPichincha)
            return RespuestaDto<string>.Fallo("Este pedido no requiere comprobante de transferencia.", 400);

        var resultado = await _archivos.GuardarComprobanteAsync(req.Contenido, req.NombreOriginal, req.ContentType, req.TamanoBytes);
        if (!resultado.Exito) return RespuestaDto<string>.Fallo(resultado.Error!, 400);

        await _ventas.ActualizarComprobanteAsync(req.VentaId, resultado.Url!, EstadosPago.PendienteVerificacion);

        return RespuestaDto<string>.Ok(resultado.Url!, "Comprobante subido correctamente. Tu pedido está en verificación.");
    }
}
