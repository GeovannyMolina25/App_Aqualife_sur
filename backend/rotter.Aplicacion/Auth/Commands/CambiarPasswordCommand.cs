using BCrypt.Net;
using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;

namespace rotter.Aplicacion.Auth.Commands;

public record CambiarPasswordCommand(
    int UsuarioId,
    string NuevaPassword
) : IRequest<RespuestaDto<bool>>;

public class CambiarPasswordHandler
    : IRequestHandler<
        CambiarPasswordCommand,
        RespuestaDto<bool>
    >
{
    private readonly IUsuarioRepositorio _usuarios;

    public CambiarPasswordHandler(
        IUsuarioRepositorio usuarios
    )
    {
        _usuarios = usuarios;
    }

    public async Task<RespuestaDto<bool>> Handle(
        CambiarPasswordCommand req,
        CancellationToken ct
    )
    {
        var usuario =
            await _usuarios.ObtenerPorIdAsync(
                req.UsuarioId
            );

        if (usuario is null)
        {
            return RespuestaDto<bool>.Fallo(
                "Usuario no encontrado.",
                404
            );
        }

        usuario.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(
                req.NuevaPassword
            );

        await _usuarios.ActualizarAsync(usuario);

        return RespuestaDto<bool>.Ok(
            true,
            "Contraseña actualizada."
        );
    }
}