namespace rotter.Dominio.DTOs.Ventas;

public record CheckoutDto(
    List<ItemVentaDto> Items,
    string MetodoPago,
    string DireccionEnvio,
    string TelefonoContacto,
    string NombreReceptor,
    string? Observacion
);

public static class MetodosPago
{
    public const string TransferenciaProdubanco = "TransferenciaProdubanco";
    public const string TransferenciaPichincha = "TransferenciaPichincha";
    public const string ContraEntrega = "ContraEntrega";

    public static readonly HashSet<string> Validos = new() { TransferenciaProdubanco, TransferenciaPichincha, ContraEntrega };
}

public static class EstadosPago
{
    public const string PendienteVerificacion = "PendienteVerificacion";
    public const string Verificado = "Verificado";
    public const string Rechazado = "Rechazado";
    public const string ContraEntrega = "ContraEntrega";
}
