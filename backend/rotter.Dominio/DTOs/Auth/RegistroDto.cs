namespace rotter.Dominio.DTOs.Auth;

public record RegistroDto(
    string Nombre,
    string Apellido,
    string Email,
    string Password,
    DateOnly FechaNacimiento,
    string Sexo,
    string Direccion,
    string? Telefono
);
