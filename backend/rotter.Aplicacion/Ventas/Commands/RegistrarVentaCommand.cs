using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;
using rotter.Infraestructura.Data;

namespace rotter.Aplicacion.Ventas.Commands;

public record RegistrarVentaCommand(CrearVentaDto Dto, int ColaboradorId) : IRequest<RespuestaDto<VentaDto>>;

public class RegistrarVentaHandler : IRequestHandler<RegistrarVentaCommand, RespuestaDto<VentaDto>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IProductoRepositorio _productos;
    private readonly IAuditoriaServicio _auditoria;
    private readonly RotterDbContext _db;

    public RegistrarVentaHandler(IVentaRepositorio ventas, IProductoRepositorio productos, IAuditoriaServicio auditoria, RotterDbContext db)
    { _ventas = ventas; _productos = productos; _auditoria = auditoria; _db = db; }

    public async Task<RespuestaDto<VentaDto>> Handle(RegistrarVentaCommand req, CancellationToken ct)
    {
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        var numero = await _ventas.GenerarNumeroVentaAsync();
        var construido = await ConstructorDetallesVenta.ConstruirAsync(_productos, req.Dto.Items);
        if (construido.Error is not null) return RespuestaDto<VentaDto>.Fallo(construido.Error, construido.ErrorEsNotFound ? 404 : 400);

        var venta = new Venta
        {
            NumeroVenta = numero,
            ClienteId = req.Dto.ClienteId,
            ColaboradorId = req.ColaboradorId,
            Subtotal = construido.Subtotal,
            Impuestos = 0,
            Total = construido.Subtotal,
            Observacion = req.Dto.Observacion,
            Estado = "Pendiente",
            Origen = "Presencial",
            FechaVenta = DateTime.Now,
            Detalles = construido.Detalles
        };

        await _ventas.CrearAsync(venta);
        await tx.CommitAsync(ct);

        await _auditoria.RegistrarAsync("REGISTRAR_VENTA", "Ventas", venta.Id.ToString(),
            datosNuevos: new {
                venta.NumeroVenta,
                venta.Total
            });

        var creada = await _ventas.ObtenerPorIdAsync(venta.Id);
        return RespuestaDto<VentaDto>.Ok(Mapear(creada!), "Venta registrada exitosamente.");
    }

    public static VentaDto Mapear(Venta v) => new(
        v.Id,
        v.NumeroVenta,
        v.FechaVenta,
        v.Subtotal,
        v.Impuestos,
        v.Total,
        v.Estado,
        v.Origen,
        v.MetodoPago,
        v.EstadoPago,
        v.ComprobanteUrl,
        v.DireccionEnvio,
        v.TelefonoContacto,
        v.NombreReceptor,
        v.Observacion,
        $"{v.Cliente.Nombre} {v.Cliente.Apellido}",
        v.Cliente.Email,
        $"{v.Colaborador.Nombre} {v.Colaborador.Apellido}",
        v.Detalles.Select(d => new DetalleVentaDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal)).ToList());
}
