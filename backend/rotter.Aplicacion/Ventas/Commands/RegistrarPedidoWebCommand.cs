using MediatR;
using rotter.Dominio.Constantes;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;
using rotter.Infraestructura.Data;

namespace rotter.Aplicacion.Ventas.Commands;

public record RegistrarPedidoWebCommand(CheckoutDto Dto, int ClienteId) : IRequest<RespuestaDto<VentaDto>>;

/// <summary>
/// Checkout de autoservicio para el catálogo público: el cliente ya autenticado arma su propio
/// pedido (sin colaborador real detrás), paga por transferencia bancaria o contra entrega, y el
/// pedido queda registrado como una Venta con Origen="Web".
/// </summary>
public class RegistrarPedidoWebHandler : IRequestHandler<RegistrarPedidoWebCommand, RespuestaDto<VentaDto>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IProductoRepositorio _productos;
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio _auditoria;
    private readonly IEmailServicio _email;
    private readonly RotterDbContext _db;

    public RegistrarPedidoWebHandler(IVentaRepositorio ventas, IProductoRepositorio productos, IUsuarioRepositorio usuarios,
        IAuditoriaServicio auditoria, IEmailServicio email, RotterDbContext db)
    { _ventas = ventas; _productos = productos; _usuarios = usuarios; _auditoria = auditoria; _email = email; _db = db; }

    public async Task<RespuestaDto<VentaDto>> Handle(RegistrarPedidoWebCommand req, CancellationToken ct)
    {
        if (!MetodosPago.Validos.Contains(req.Dto.MetodoPago))
            return RespuestaDto<VentaDto>.Fallo("Método de pago no válido.", 400);

        if (req.Dto.Items.Count == 0)
            return RespuestaDto<VentaDto>.Fallo("El carrito está vacío.", 400);

        if (string.IsNullOrWhiteSpace(req.Dto.DireccionEnvio) || string.IsNullOrWhiteSpace(req.Dto.TelefonoContacto) || string.IsNullOrWhiteSpace(req.Dto.NombreReceptor))
            return RespuestaDto<VentaDto>.Fallo("Completa la dirección de envío, el teléfono y el nombre de quien recibe el pedido.", 400);

        var sistemaColaborador = await _usuarios.ObtenerPorEmailAsync(EmpresaConstantes.EmailUsuarioSistemaWeb);
        if (sistemaColaborador is null)
            return RespuestaDto<VentaDto>.Fallo("El sistema de pedidos web no está configurado. Contacta al administrador.", 500);

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var numero = await _ventas.GenerarNumeroVentaAsync();
        var construido = await ConstructorDetallesVenta.ConstruirAsync(_productos, req.Dto.Items);
        if (construido.Error is not null) return RespuestaDto<VentaDto>.Fallo(construido.Error, construido.ErrorEsNotFound ? 404 : 400);

        var impuestos = Math.Round(construido.Subtotal * EmpresaConstantes.IvaPedidosWeb, 2);
        var total = construido.Subtotal + impuestos;
        var estadoPago = req.Dto.MetodoPago == MetodosPago.ContraEntrega ? EstadosPago.ContraEntrega : EstadosPago.PendienteVerificacion;

        var venta = new Venta
        {
            NumeroVenta = numero,
            ClienteId = req.ClienteId,
            ColaboradorId = sistemaColaborador.Id,
            Subtotal = construido.Subtotal,
            Impuestos = impuestos,
            Total = total,
            Observacion = req.Dto.Observacion,
            Estado = "Pendiente",
            Origen = "Web",
            MetodoPago = req.Dto.MetodoPago,
            EstadoPago = estadoPago,
            DireccionEnvio = req.Dto.DireccionEnvio,
            TelefonoContacto = req.Dto.TelefonoContacto,
            NombreReceptor = req.Dto.NombreReceptor,
            FechaVenta = DateTime.Now,
            Detalles = construido.Detalles
        };

        await _ventas.CrearAsync(venta);
        await tx.CommitAsync(ct);

        await _auditoria.RegistrarAsync("REGISTRAR_PEDIDO_WEB", "Ventas", venta.Id.ToString(),
            datosNuevos: new { venta.NumeroVenta, venta.Total, venta.MetodoPago });

        var creada = await _ventas.ObtenerPorIdAsync(venta.Id);
        await EnviarCorreoConfirmacionAsync(creada!);

        return RespuestaDto<VentaDto>.Ok(RegistrarVentaHandler.Mapear(creada!), "Pedido registrado exitosamente.");
    }

    private async Task EnviarCorreoConfirmacionAsync(Venta venta)
    {
        try
        {
            var filas = string.Join("", venta.Detalles.Select(d => $@"
                <tr>
                    <td style='padding:8px;border-bottom:1px solid #eee;'>{d.Producto.Nombre}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:center;'>{d.Cantidad}</td>
                    <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>${d.Subtotal:0.00}</td>
                </tr>"));

            await _email.EnviarAsync(
                venta.Cliente.Email,
                $"Confirmación de pedido {venta.NumeroVenta} — {EmpresaConstantes.Nombre}",
                $@"
                <div style='font-family: Arial; padding: 20px; background: #f5f5f5;'>
                    <div style='max-width: 500px; margin: auto; background: white; border-radius: 12px; padding: 30px; box-shadow: 0 2px 10px rgba(0,0,0,.08);'>
                        <h2 style='color:#1a6b8a;'>¡Gracias por tu pedido!</h2>
                        <p>Hola <strong>{venta.Cliente.Nombre}</strong>, recibimos tu pedido <strong>{venta.NumeroVenta}</strong>.</p>
                        <table style='width:100%;border-collapse:collapse;margin:16px 0;'>
                            <thead><tr><th style='text-align:left;padding:8px;'>Producto</th><th style='padding:8px;'>Cant.</th><th style='text-align:right;padding:8px;'>Subtotal</th></tr></thead>
                            <tbody>{filas}</tbody>
                        </table>
                        <p>Subtotal: ${venta.Subtotal:0.00}<br/>IVA: ${venta.Impuestos:0.00}<br/><strong>Total: ${venta.Total:0.00}</strong></p>
                        <div style='background:#f5f5f5;border-radius:8px;padding:14px 16px;margin:16px 0;font-size:14px;'>
                            <strong>Datos de entrega</strong><br/>
                            Dirección: {venta.DireccionEnvio}<br/>
                            Quien recibe: {venta.NombreReceptor}<br/>
                            Teléfono: {venta.TelefonoContacto}
                        </div>
                        <p style='color:#666;font-size:14px;'>
                            {(venta.MetodoPago == "ContraEntrega"
                                ? "Pagarás contra entrega. Te contactaremos para coordinar."
                                : "Tu pedido está pendiente de verificación de pago. Revisaremos tu comprobante y te confirmaremos pronto.")}
                        </p>
                        <p style='color:#999;font-size:12px;'>{EmpresaConstantes.Slogan}</p>
                    </div>
                </div>"
            );
        }
        catch
        {
            // Un fallo de SMTP no debe hacer fallar un pedido ya creado y con stock ya descontado.
        }
    }
}
