namespace rotter.Dominio.DTOs.Usuarios;

public record CrearUsuarioRapidoDto(
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Direccion
);

public record UsuarioRapidoCreadoDto(int Id, string Nombre, string Apellido, string Email, string Telefono);
