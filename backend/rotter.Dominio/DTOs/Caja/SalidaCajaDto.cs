namespace rotter.Dominio.DTOs.Caja;

public record CrearSalidaCajaDto(
    decimal Valor,
    string Motivo,
    string? Descripcion,
    string? Banco,
    string? NumeroTransaccion,
    string? ComprobanteUrl
);

public record SalidaCajaDto(
    int Id,
    string NumeroSalida,
    DateTime FechaSalida,
    decimal Valor,
    string? Banco,
    string? NumeroTransaccion,
    string? ComprobanteUrl,
    string Motivo,
    string? Descripcion,
    string NombreUsuario
);
