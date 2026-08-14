namespace rotter.Dominio.DTOs.Productos;

public record ProductoDto(
    int Id,
    string Nombre,
    string? Descripcion,
    string? Caracteristicas,
    decimal Precio,
    int Stock,
    int CategoriaId,
    string Categoria,
    string CategoriaTipo,
    string? ImagenUrl,
    bool EsPromocion,
    decimal? PrecioPromocion,
    DateTime? FechaInicioPromocion,
    DateTime? FechaFinPromocion,
    bool Activo
);
