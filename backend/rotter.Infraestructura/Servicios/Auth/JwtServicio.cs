using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Auth;

public class JwtServicio : IJwtServicio
{
    private readonly IConfiguration _config;
    public JwtServicio(IConfiguration config) => _config = config;

    public string GenerarToken(Usuario usuario)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var exp   = DateTime.UtcNow.AddHours(int.Parse(_config["JWT:HorasExpiracion"] ?? "24"));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email,           usuario.Email),
            new Claim(ClaimTypes.Name,            $"{usuario.Nombre} {usuario.Apellido}"),
            new Claim(ClaimTypes.Role,            usuario.Rol.Nombre),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["JWT:Issuer"], audience: _config["JWT:Audience"],
            claims: claims, expires: exp, signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerarRefreshToken()
    {
        var bytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public ClaimsPrincipal? ValidarToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]!));
            return new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                ValidateIssuer = true,  ValidIssuer   = _config["JWT:Issuer"],
                ValidateAudience = true, ValidAudience = _config["JWT:Audience"],
                ValidateLifetime = false
            }, out _);
        }
        catch { return null; }
    }
}
