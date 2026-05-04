namespace rotter.Dominio.DTOs.Productos;

public record CrearProductoDto(
    string Nombre,
    string? Descripcion,
    string? Caracteristicas,
    decimal Precio,
    int Stock,
    int CategoriaId,
    string? ImagenUrl,
    bool EsPromocion,
    decimal? PrecioPromocion,
    DateTime? FechaInicioPromocion,
    DateTime? FechaFinPromocion
);
