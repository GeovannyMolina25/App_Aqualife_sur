using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Servicios;

public interface IEmisorSesionServicio
{
    Task<AuthResponseDto> EmitirAsync(Usuario usuario);
}
