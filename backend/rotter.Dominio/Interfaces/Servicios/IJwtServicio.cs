using System.Security.Claims;
using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Servicios;

public interface IJwtServicio
{
    string GenerarToken(Usuario usuario);
    string GenerarRefreshToken();
    string HashRefreshToken(string refreshToken);
    int ObtenerMinutosExpiracion();
    ClaimsPrincipal? ValidarToken(string token);
    string GenerarTokenDesafio2FA(int usuarioId);
    int? ValidarTokenDesafio2FA(string token);
}
