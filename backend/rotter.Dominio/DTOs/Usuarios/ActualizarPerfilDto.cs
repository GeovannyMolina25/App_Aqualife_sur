namespace rotter.Dominio.DTOs.Usuarios;

public record ActualizarPerfilDto(
    string Nombre,
    string Apellido,
    string Direccion,
    string? Telefono
);
