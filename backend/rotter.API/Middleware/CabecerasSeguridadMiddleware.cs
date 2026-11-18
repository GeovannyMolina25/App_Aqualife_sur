namespace rotter.API.Middleware;

public class CabecerasSeguridadMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public CabecerasSeguridadMiddleware(RequestDelegate next, IHostEnvironment env)
    { _next = next; _env = env; }

    public async Task InvokeAsync(HttpContext ctx)
    {
        ctx.Response.OnStarting(() =>
        {
            var h = ctx.Response.Headers;
            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"]        = "DENY";
            h["Referrer-Policy"]        = "strict-origin-when-cross-origin";
            h["Permissions-Policy"]     = "geolocation=(), camera=(), microphone=()";

            if (!_env.IsDevelopment())
                h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

            return Task.CompletedTask;
        });

        await _next(ctx);
    }
}
