using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;
using rotter.Infraestructura.Data;
using rotter.Infraestructura.Repositorios.Auditoria;
using rotter.Infraestructura.Repositorios.Productos;
using rotter.Infraestructura.Repositorios.Usuarios;
using rotter.Infraestructura.Repositorios.Ventas;
using rotter.Infraestructura.Servicios.Auth;
using rotter.Infraestructura.Servicios.Auditoria;
using rotter.Infraestructura.Servicios.Reportes;

namespace rotter.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddBaseDatos(this IServiceCollection s, IConfiguration c)
    {
        var connectionString = c.GetConnectionString("DefaultConnection");

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString)
        {
            Encrypt = false,
            TrustServerCertificate = true
        };

        s.AddDbContext<RotterDbContext>(o =>
            o.UseSqlServer(builder.ConnectionString));

        return s;
    }

    public static IServiceCollection AddRepositorios(this IServiceCollection s)
    {
        s.AddScoped<IUsuarioRepositorio,   UsuarioRepositorio>();
        s.AddScoped<IProductoRepositorio,  ProductoRepositorio>();
        s.AddScoped<IVentaRepositorio,     VentaRepositorio>();
        s.AddScoped<IAuditoriaRepositorio, AuditoriaRepositorio>();
        return s;
    }

    public static IServiceCollection AddServicios(this IServiceCollection s)
    {
        s.AddScoped<IJwtServicio,       JwtServicio>();
        s.AddScoped<IAuditoriaServicio, AuditoriaServicio>();
        s.AddScoped<IReporteServicio,   ReporteServicio>();
        s.AddHttpContextAccessor();
        return s;
    }

    public static IServiceCollection AddJwtAuth(this IServiceCollection s, IConfiguration c)
    {
        var key = Encoding.UTF8.GetBytes(c["JWT:SecretKey"]!);
        s.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,  ValidIssuer   = c["JWT:Issuer"],
                ValidateAudience = true, ValidAudience = c["JWT:Audience"],
                ValidateLifetime = true, ClockSkew     = TimeSpan.Zero
            });
        s.AddAuthorization();
        return s;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection s)
    {
        s.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Rotter API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            { Description = "JWT Bearer", Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey, Scheme = "Bearer" });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{ new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }});
        });
        return s;
    }

    public static IServiceCollection AddCorsAngular(this IServiceCollection s)
    {
        s.AddCors(o => o.AddPolicy("Angular", p =>
            p.WithOrigins(
                "http://localhost:4200",
                "http://192.168.100.53:4200",
                "http://localhost:9090",              // 👈 AGREGA ESTE
                "http://192.168.100.53:9090"         // 👈 Y ESTE
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
        ));
        return s;
    }
}
