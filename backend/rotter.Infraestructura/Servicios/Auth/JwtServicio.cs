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

    public int ObtenerMinutosExpiracion() => int.Parse(_config["JWT:MinutosExpiracion"] ?? "30");

    public string GenerarToken(Usuario usuario)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var exp   = DateTime.Now.AddMinutes(ObtenerMinutosExpiracion());

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

    public string HashRefreshToken(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    public string GenerarTokenDesafio2FA(int usuarioId)
    {
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("uid", usuarioId.ToString()),
            new Claim("purpose", "2fa")
        };

        var token = new JwtSecurityToken(
            issuer: _config["JWT:Issuer"], audience: _config["JWT:Audience"],
            claims: claims, expires: DateTime.Now.AddMinutes(5), signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidarTokenDesafio2FA(string token)
    {
        var principal = ValidarToken(token);
        if (principal is null) return null;

        var purpose = principal.FindFirst("purpose")?.Value;
        var uid     = principal.FindFirst("uid")?.Value;
        if (purpose != "2fa" || uid is null) return null;

        var exp = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
        if (exp is null || DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)) < DateTimeOffset.UtcNow) return null;

        return int.Parse(uid);
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
