namespace rotter.Dominio.DTOs.Auth;

public record LoginResultadoDto(
    bool RequiereDosFactores,
    string? TokenDesafio,
    AuthResponseDto? Auth
);
