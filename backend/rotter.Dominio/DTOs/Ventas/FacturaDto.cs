namespace rotter.Dominio.DTOs.Ventas;

public class FacturaDto
{
    public string NumeroFactura { get; set; } = "";

    public DateTime Fecha { get; set; }

    public string Cliente { get; set; } = "";

    public string? CedulaCliente { get; set; }

    public string EmailCliente { get; set; } = "";

    public string DireccionCliente { get; set; } = "";

    public decimal Subtotal { get; set; }

    public decimal Iva { get; set; }

    public decimal Total { get; set; }

    public List<FacturaDetalleDto> Detalles { get; set; }
        = [];
}