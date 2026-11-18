namespace rotter.Dominio.DTOs.Ventas;

public record MetricasDto(
    decimal TotalVentasHoy,
    decimal TotalVentasMes,
    int VentasHoy,
    int ProductosActivos,
    List<TopColaboradorDto> TopColaboradores,
    List<TopProductoDto> TopProductos,
    List<TopProductoDto> TopServicios,
    List<TopClienteDto> TopClientes
);

public record TopColaboradorDto(string Nombre, int TotalVentas, decimal MontoTotal);
public record TopProductoDto(string Nombre, int Unidades, decimal MontoTotal);
public record TopClienteDto(string Nombre, string Email, int Compras, int Unidades, decimal TotalGastado);
