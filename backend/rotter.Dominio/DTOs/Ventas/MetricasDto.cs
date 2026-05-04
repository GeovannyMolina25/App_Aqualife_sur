namespace rotter.Dominio.DTOs.Ventas;

public record MetricasDto(
    decimal TotalVentasHoy,
    decimal TotalVentasMes,
    int VentasHoy,
    int ProductosActivos,
    List<TopColaboradorDto> TopColaboradores
);

public record TopColaboradorDto(string Nombre, int TotalVentas, decimal MontoTotal);
