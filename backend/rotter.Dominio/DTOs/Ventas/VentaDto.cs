namespace rotter.Dominio.DTOs.Ventas;

public record VentaDto(
    int Id,
    string NumeroVenta,
    DateTime FechaVenta,
    decimal Subtotal,
    decimal Impuestos,
    decimal Total,
    string Estado,
    string Origen,
    string? MetodoPago,
    string? EstadoPago,
    string? ComprobanteUrl,
    string? DireccionEnvio,
    string? TelefonoContacto,
    string? NombreReceptor,
    string? Observacion,
    string NombreCliente,
    string EmailCliente,
    string NombreColaborador,
    List<DetalleVentaDto> Detalles
);
