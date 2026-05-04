namespace rotter.Dominio.DTOs.Ventas;

public record VentaDto(
    int Id,
    string NumeroVenta,
    DateTime FechaVenta,
    decimal Total,
    string Estado,
    string? Observacion,
    string NombreCliente,
    string EmailCliente,
    string NombreColaborador,
    List<DetalleVentaDto> Detalles
);
