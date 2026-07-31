namespace rotter.Dominio.DTOs.Caja;

public record SaldoCajaDto(
    decimal TotalIngresos,
    decimal TotalSalidas,
    decimal Saldo
);

public record MovimientoCajaDto(
    int Id,
    string Tipo,
    string Numero,
    DateTime Fecha,
    decimal Valor,
    string NombreUsuario,
    string? Detalle
);
