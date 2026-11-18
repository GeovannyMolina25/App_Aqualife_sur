using System.Net;
using System.Text.Json;
using rotter.Dominio.DTOs.Comun;

namespace rotter.API.Middleware;

public class ExcepcionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExcepcionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExcepcionMiddleware(RequestDelegate next, ILogger<ExcepcionMiddleware> logger, IHostEnvironment env)
    { _next = next; _logger = logger; _env = env; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (FluentValidation.ValidationException vex)
        {
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode  = (int)HttpStatusCode.BadRequest;
            var errores = vex.Errors.Select(e => e.ErrorMessage).ToList();
            var res = RespuestaDto<object>.Fallo("Datos inválidos.", 400, errores);
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(res, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
        catch (Exception ex)
        {
            var traceId = ctx.TraceIdentifier;
            _logger.LogError(ex, "Error no controlado ({TraceId}): {Msg}", traceId, ex.Message);

            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;

            var detalle = _env.IsDevelopment() ? new List<string> { ex.Message } : new List<string>();
            var res = RespuestaDto<object>.Fallo($"Error interno del servidor. (ref: {traceId})", 500, detalle);
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(res, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
