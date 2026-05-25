using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Ventas.Commands;

public record RegistrarVentaCommand(CrearVentaDto Dto, int ColaboradorId) : IRequest<RespuestaDto<VentaDto>>;

public class RegistrarVentaHandler : IRequestHandler<RegistrarVentaCommand, RespuestaDto<VentaDto>>
{
    private readonly IVentaRepositorio _ventas;
    private readonly IProductoRepositorio _productos;
    private readonly IAuditoriaServicio _auditoria;

    public RegistrarVentaHandler(IVentaRepositorio ventas, IProductoRepositorio productos, IAuditoriaServicio auditoria)
    { _ventas = ventas; _productos = productos; _auditoria = auditoria; }

    public async Task<RespuestaDto<VentaDto>> Handle(RegistrarVentaCommand req, CancellationToken ct)
    {
        var numero = await _ventas.GenerarNumeroVentaAsync();
        var detalles = new List<DetalleVenta>();
        decimal total = 0;

        foreach (var item in req.Dto.Items)
        {
            var prod = await _productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod is null) return RespuestaDto<VentaDto>.Fallo($"Producto {item.ProductoId} no encontrado.", 404);
            if (prod.Stock < item.Cantidad) return RespuestaDto<VentaDto>.Fallo($"Stock insuficiente: {prod.Nombre}.", 400);

            var precio = prod.EsPromocion && prod.PrecioPromocion.HasValue ? prod.PrecioPromocion.Value : prod.Precio;
            var subtotal = precio * item.Cantidad;
            total += subtotal;

            detalles.Add(new DetalleVenta { ProductoId = item.ProductoId, Cantidad = item.Cantidad, PrecioUnitario = precio, Subtotal = subtotal });
            await _productos.ActualizarStockAsync(item.ProductoId, item.Cantidad);
        }

        var venta = new Venta
        {
            NumeroVenta = numero,
            ClienteId = req.Dto.ClienteId,
            ColaboradorId = req.ColaboradorId,
            Total = total,
            Observacion = req.Dto.Observacion,
            Estado = "Pendiente",
            FechaVenta = DateTime.Now,
            Detalles = detalles
        };

        await _ventas.CrearAsync(venta);
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
        v.Total, 
        v.Estado, 
        v.Observacion,
        $"{v.Cliente.Nombre} {v.Cliente.Apellido}", 
        v.Cliente.Email,
        $"{v.Colaborador.Nombre} {v.Colaborador.Apellido}",
        v.Detalles.Select(d => new DetalleVentaDto(d.ProductoId, d.Producto.Nombre, d.Cantidad, d.PrecioUnitario, d.Subtotal)).ToList());
}
