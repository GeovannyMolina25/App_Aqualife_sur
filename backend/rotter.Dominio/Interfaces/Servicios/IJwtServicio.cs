using System.Security.Claims;
using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Servicios;

public interface IJwtServicio
{
    string GenerarToken(Usuario usuario);
    string GenerarRefreshToken();
    ClaimsPrincipal? ValidarToken(string token);
}
