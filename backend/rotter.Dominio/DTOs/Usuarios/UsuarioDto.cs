namespace rotter.Dominio.DTOs.Usuarios;

public record UsuarioDto(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    DateOnly FechaNacimiento,
    string Sexo,
    string Direccion,
    string? Telefono,
    string Rol,
    bool Activo,
    DateTime FechaCreacion,
    string? PremioBienvenida,
    int RecargasParaSeptimo,
    bool PremioBienvenidaEntregado
);
