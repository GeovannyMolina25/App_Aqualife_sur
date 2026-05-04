namespace rotter.Dominio.DTOs.Ventas;

public record CrearVentaDto(
    int ClienteId,
    string? Observacion,
    List<ItemVentaDto> Items
);

public record ItemVentaDto(int ProductoId, int Cantidad);
