using Microsoft.EntityFrameworkCore;
using rotter.API.Extensions;
using rotter.API.Middleware;
using rotter.Dominio.Interfaces.Servicios;
using rotter.Infraestructura.Data;
using rotter.Infraestructura.Servicios;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<
    IEmailServicio,
    EmailServicio
>();
builder.Services.AddBaseDatos(builder.Configuration);
builder.Services.AddRepositorios();
builder.Services.AddServicios();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddCorsAngular(); 


builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("rotter.Aplicacion"))
);
Console.WriteLine(
    builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("Angular");             
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