using System.Net;
using System.Text.Json;
using rotter.Dominio.DTOs.Comun;

namespace rotter.API.Middleware;

public class ExcepcionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExcepcionMiddleware> _logger;

    public ExcepcionMiddleware(RequestDelegate next, ILogger<ExcepcionMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado: {Msg}", ex.Message);
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode  = (int)HttpStatusCode.InternalServerError;
            var res = RespuestaDto<object>.Fallo("Error interno del servidor.", 500, new() { ex.Message });
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(res, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
