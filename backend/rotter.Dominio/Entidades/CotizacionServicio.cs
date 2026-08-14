namespace rotter.Dominio.Entidades;

public static class EstadosCotizacion
{
    /// <summary>Recién ingresada, el personal todavía no la ha revisado.</summary>
    public const string Entrante = "Entrante";
    /// <summary>El personal ya la revisó/contactó al cliente y está gestionándola.</summary>
    public const string Pendiente = "Pendiente";
    /// <summary>El servicio se realizó/completó.</summary>
    public const string Realizado = "Realizado";
    /// <summary>No se puede brindar ese servicio.</summary>
    public const string Anulado = "Anulado";
}

public class CotizacionServicio
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public int? ClienteId { get; set; }
    public string NombreContacto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string TamanoEspacio { get; set; } = string.Empty;
    public DateTime? FechaDeseada { get; set; }
    public string? Comentario { get; set; }
    public string Estado { get; set; } = EstadosCotizacion.Entrante;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }

    public Producto Producto { get; set; } = null!;
    public Usuario? Cliente { get; set; }
}
