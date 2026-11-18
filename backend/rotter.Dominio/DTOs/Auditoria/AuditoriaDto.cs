namespace rotter.Dominio.DTOs.Auditoria;

public record AuditoriaDto(
    long Id,
    int? UsuarioId,
    string? UsuarioEmail,
    string Accion,
    string Entidad,
    string? EntidadId,
    string? IpAddress,
    DateTime FechaAccion,
    bool Exitoso,
    string? MensajeError
);
