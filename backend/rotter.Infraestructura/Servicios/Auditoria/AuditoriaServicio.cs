using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Auditoria;

public class AuditoriaServicio : IAuditoriaServicio
{
    private readonly IAuditoriaRepositorio _repo;
    private readonly IHttpContextAccessor  _http;

    public AuditoriaServicio(IAuditoriaRepositorio repo, IHttpContextAccessor http)
    { _repo = repo; _http = http; }

    public async Task RegistrarAsync(string accion, string entidad, string? entidadId = null,
        object? datosAnteriores = null, object? datosNuevos = null, bool exitoso = true, string? mensajeError = null)
    {
        var ctx = _http.HttpContext;
        var uid = ctx?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await _repo.RegistrarAsync(new rotter.Dominio.Entidades.Auditoria
        {
            UsuarioId       = uid != null ? int.Parse(uid) : null,
            UsuarioEmail    = ctx?.User?.FindFirst(ClaimTypes.Email)?.Value,
            Accion          = accion,
            Entidad         = entidad,
            EntidadId       = entidadId,
            DatosAnteriores = datosAnteriores != null ? JsonSerializer.Serialize(datosAnteriores) : null,
            DatosNuevos     = datosNuevos     != null ? JsonSerializer.Serialize(datosNuevos)     : null,
            IpAddress       = ctx?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent       = ctx?.Request?.Headers["User-Agent"].ToString(),
            FechaAccion     = DateTime.Now,
            Exitoso         = exitoso,
            MensajeError    = mensajeError
        });
    }
}
