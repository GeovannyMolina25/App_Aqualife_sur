using System.Reflection;
using rotter.API.Extensions;
using rotter.API.Middleware;
using rotter.Infraestructura.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

Console.WriteLine("=================================");
Console.WriteLine(
    builder.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("=================================");
// 🔹 Servicios personalizados
builder.Services.AddBaseDatos(builder.Configuration);
builder.Services.AddRepositorios();
builder.Services.AddServicios();
builder.Services.AddJwtAuth(builder.Configuration);
builder.Services.AddCorsAngular(); // 👈 TU MÉTODO



// 🔹 Otros servicios
builder.Services.AddSwagger();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("rotter.Aplicacion"))
);
Console.WriteLine(
    builder.Configuration.GetConnectionString("DefaultConnection"));

var app = builder.Build();


// 🔹 Swagger
app.UseSwagger();
app.UseSwaggerUI();

// 🔥 ORDEN CORRECTO (CLAVE)
app.UseCors("Angular");                // 👈 PRIMERO
app.UseMiddleware<ExcepcionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 🔹 Migraciones automáticas
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider
        .GetRequiredService<RotterDbContext>()
        .Database.MigrateAsync();
}

// 🔹 Debug
Console.WriteLine("JWT KEY: " + builder.Configuration["JWT:SecretKey"]);

app.Run();