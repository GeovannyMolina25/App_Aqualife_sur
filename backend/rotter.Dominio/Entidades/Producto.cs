namespace rotter.Dominio.Entidades;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Caracteristicas { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int CategoriaId { get; set; }
    public string? ImagenUrl { get; set; }
    public bool EsPromocion { get; set; } = false;
    public decimal? PrecioPromocion { get; set; }
    public DateTime? FechaInicioPromocion { get; set; }
    public DateTime? FechaFinPromocion { get; set; }
    public bool Activo { get; set; } = true;
    public int CreadoPorId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaModificacion { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public Usuario CreadoPor { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
