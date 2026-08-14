namespace rotter.Dominio.Entidades;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateOnly FechaNacimiento { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }

    // Promoción de bienvenida (ruleta + tarjeta de 7º botellón gratis)
    public string? PremioBienvenida { get; set; }
    public DateTime? FechaPremioBienvenida { get; set; }
    public int RecargasParaSeptimo { get; set; } = 0;
    public bool PremioBienvenidaEntregado { get; set; } = false;

    public Rol Rol { get; set; } = null!;
    public ICollection<Venta> VentasComoCliente { get; set; } = new List<Venta>();
    public ICollection<Venta> VentasComoColaborador { get; set; } = new List<Venta>();
}
