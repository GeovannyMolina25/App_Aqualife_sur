namespace rotter.Dominio.DTOs.Cotizaciones;

public record CrearCotizacionServicioDto(
    int ProductoId,
    string NombreContacto,
    string Telefono,
    string? Email,
    string Direccion,
    string TamanoEspacio,
    DateTime? FechaDeseada,
    string? Comentario
);

public record CotizacionServicioDto(
    int Id,
    int ProductoId,
    string ServicioNombre,
    string NombreContacto,
    string Telefono,
    string? Email,
    string Direccion,
    string TamanoEspacio,
    DateTime? FechaDeseada,
    string? Comentario,
    string Estado,
    DateTime FechaCreacion
);

public record CambiarEstadoCotizacionDto(string Estado);
