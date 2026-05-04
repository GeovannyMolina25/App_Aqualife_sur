namespace rotter.Dominio.Entidades;

public class Auditoria
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string? UsuarioEmail { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Entidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? DatosAnteriores { get; set; }
    public string? DatosNuevos { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime FechaAccion { get; set; } = DateTime.UtcNow;
    public bool Exitoso { get; set; } = true;
    public string? MensajeError { get; set; }
}
