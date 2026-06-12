namespace rotter.Dominio.DTOs.Ventas;

public record FacturaDetalleDto(
    string Producto,
    string? Caracteristicas,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);