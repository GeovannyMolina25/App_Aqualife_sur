namespace rotter.Dominio.DTOs.Caja;

public record CrearIngresoCajaDto(
    string Banco,
    string NumeroTransaccion,
    decimal TotalIngresado,
    string? Observacion,
    string? ComprobanteUrl,
    List<int>? VentaIds
);

public record IngresoCajaDto(
    int Id,
    string NumeroIngreso,
    string Banco,
    string NumeroTransaccion,
    string? ComprobanteUrl,
    decimal TotalIngresado,
    string? Observacion,
    string NombreUsuario,
    DateTime FechaIngreso,
    List<int> VentaIds
);
