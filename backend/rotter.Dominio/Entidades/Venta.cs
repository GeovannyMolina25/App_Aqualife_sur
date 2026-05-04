namespace rotter.Dominio.Entidades;

public class Venta
{
    public int Id { get; set; }
    public string NumeroVenta { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int ColaboradorId { get; set; }
    public decimal Total { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; } = "Completada";
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public Usuario Cliente { get; set; } = null!;
    public Usuario Colaborador { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
