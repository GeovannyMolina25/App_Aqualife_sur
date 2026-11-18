using FluentValidation;
using Microsoft.EntityFrameworkCore;
using rotter.API.Extensions;
using rotter.API.Middleware;
using rotter.Aplicacion.Comun.Comportamientos;
using rotter.Dominio.Interfaces.Servicios;
using rotter.Infraestructura.Data;
using rotter.Infraestructura.Servicios;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
// No anunciar el servidor web en las respuestas (cabecera "Server: Kestrel").
builder.WebHost.ConfigureKestrel(o => o.AddServerHeader = false);

builder.Services.AddScoped<
    IEmailServicio,
    EmailServicio
>();
builder.Services.AddBaseDatos(builder.Configuration);
builder.Services.AddRepositorios();
builder.Services.AddServicios();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddRateLimitingSeguridad();
builder.Services.AddCorsAngular(builder.Configuration);
builder.Services.AddForwardedHeadersSeguros(builder.Configuration);


builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.Load("rotter.Aplicacion"));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(Assembly.Load("rotter.Aplicacion"));

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseMiddleware<CabecerasSeguridadMiddleware>();

app.UseCors("Angular");
app.UseRateLimiter();
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "comprobantes"));
Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads", "servicios"));
app.UseStaticFiles();
app.UseMiddleware<ExcepcionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider
        .GetRequiredService<RotterDbContext>()
        .Database.MigrateAsync();
}


app.Run();