namespace rotter.Dominio.Entidades;

public class IngresoCaja
{
    public int Id { get; set; }
    public string NumeroIngreso { get; set; } = string.Empty;
    public string Banco { get; set; } = string.Empty;
    public string NumeroTransaccion { get; set; } = string.Empty;
    public string? ComprobanteUrl { get; set; }
    public decimal TotalIngresado { get; set; }
    public string? Observacion { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleIngresoCaja> Detalles { get; set; } = new List<DetalleIngresoCaja>();
}
