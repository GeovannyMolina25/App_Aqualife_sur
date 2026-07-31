namespace rotter.Dominio.Entidades;

public class DetalleIngresoCaja
{
    public int Id { get; set; }
    public int IngresoCajaId { get; set; }
    public int VentaId { get; set; }

    public IngresoCaja IngresoCaja { get; set; } = null!;
    public Venta Venta { get; set; } = null!;
}
