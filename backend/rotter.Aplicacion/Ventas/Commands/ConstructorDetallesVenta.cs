using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Ventas.Commands;

public record ResultadoConstruccionVenta(List<DetalleVenta> Detalles, decimal Subtotal, string? Error, bool ErrorEsNotFound);

/// <summary>
/// Valida stock, resuelve precios (con promoción vigente) y decrementa el stock de forma atómica.
/// Compartido entre RegistrarVentaCommand (staff) y RegistrarPedidoWebCommand (checkout público)
/// para no duplicar esta lógica de negocio.
/// </summary>
public static class ConstructorDetallesVenta
{
    public static async Task<ResultadoConstruccionVenta> ConstruirAsync(IProductoRepositorio productos, List<ItemVentaDto> items)
    {
        var detalles = new List<DetalleVenta>();
        decimal subtotal = 0;

        foreach (var item in items)
        {
            var prod = await productos.ObtenerPorIdAsync(item.ProductoId);
            if (prod is null)
                return new ResultadoConstruccionVenta(detalles, subtotal, $"Producto {item.ProductoId} no encontrado.", true);

            var precio = prod.EsPromocion && prod.PrecioPromocion.HasValue ? prod.PrecioPromocion.Value : prod.Precio;
            var itemSubtotal = precio * item.Cantidad;

            var stockOk = await productos.ActualizarStockAsync(item.ProductoId, item.Cantidad);
            if (!stockOk)
                return new ResultadoConstruccionVenta(detalles, subtotal, $"Stock insuficiente: {prod.Nombre}.", false);

            subtotal += itemSubtotal;
            detalles.Add(new DetalleVenta { ProductoId = item.ProductoId, Cantidad = item.Cantidad, PrecioUnitario = precio, Subtotal = itemSubtotal });
        }

        return new ResultadoConstruccionVenta(detalles, subtotal, null, false);
    }
}
