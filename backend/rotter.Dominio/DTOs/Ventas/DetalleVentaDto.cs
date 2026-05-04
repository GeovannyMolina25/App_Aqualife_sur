namespace rotter.Dominio.DTOs.Ventas;

public record DetalleVentaDto(
    int ProductoId,
    string NombreProducto,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);
