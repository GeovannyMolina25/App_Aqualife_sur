namespace rotter.Dominio.DTOs.Auth;

public record AuthResponseDto(
    string Token,
    string RefreshToken,
    UsuarioAuthDto Usuario,
    DateTime Expiracion,
    bool DebeCambiarPassword
);

public record UsuarioAuthDto(
    int Id,
    string Nombre,
    string Apellido,
    string Email,
    string Rol
);
