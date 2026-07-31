namespace rotter.Dominio.Entidades;

public class SalidaCaja
{
    public int Id { get; set; }
    public string NumeroSalida { get; set; } = string.Empty;
    public DateTime FechaSalida { get; set; } = DateTime.Now;
    public decimal Valor { get; set; }
    public string? Banco { get; set; }
    public string? NumeroTransaccion { get; set; }
    public string? ComprobanteUrl { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int UsuarioId { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public bool Activo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
}
