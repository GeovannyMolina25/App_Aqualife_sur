using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Dominio.DTOs.Reportes;

/// <summary>Datos del reporte "Ranking de Ventas": clientes, productos y servicios. Fechas null = sin límite.</summary>
public record RankingReporteDto(
    DateTime? Desde,
    DateTime? Hasta,
    List<TopClienteData> Clientes,
    List<TopProductoData> Productos,
    List<TopProductoData> Servicios
);
