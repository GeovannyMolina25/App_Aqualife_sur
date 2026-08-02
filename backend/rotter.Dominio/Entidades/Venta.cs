namespace rotter.Dominio.Entidades;

public class Venta
{
    public int Id { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int ColaboradorId { get; set; }
    public decimal Total { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string Origen { get; set; } = "Presencial";
    public string? MetodoPago { get; set; }
    public string? EstadoPago { get; set; }
    public string? ComprobanteUrl { get; set; }
    public string? DireccionEnvio { get; set; }
    public string? TelefonoContacto { get; set; }
    public string? NombreReceptor { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.Now;
    public Usuario Cliente { get; set; } = null!;
    public Usuario Colaborador { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
