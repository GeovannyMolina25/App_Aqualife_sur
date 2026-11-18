namespace rotter.Dominio.Entidades;

public static class TiposCategoria
{
    public const string Producto = "Producto";
    public const string Servicio = "Servicio";
}

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    /// <summary>"Producto" (se compra desde el carrito) o "Servicio" (se cotiza, sin stock/carrito).</summary>
    public string Tipo { get; set; } = TiposCategoria.Producto;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
