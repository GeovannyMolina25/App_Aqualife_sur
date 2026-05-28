using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record RecuperarPasswordCommand(
    string Dato
) : IRequest<RespuestaDto<bool>>;

public class RecuperarPasswordHandler
    : IRequestHandler<
        RecuperarPasswordCommand,
        RespuestaDto<bool>
      >
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IEmailServicio _email;

    public RecuperarPasswordHandler(
        IUsuarioRepositorio usuarios,
        IEmailServicio email
    )
    {
        _usuarios = usuarios;
        _email = email;
    }

    public async Task<RespuestaDto<bool>> Handle(
    RecuperarPasswordCommand req,
    CancellationToken ct
)
    {
        var dato = req.Dato.Trim();

        var usuario = dato.Contains("@")
            ? await _usuarios.ObtenerPorEmailAsync(dato)
            : await _usuarios.ObtenerPorTelefonoAsync(dato);

        if (usuario is null)
        {
            return RespuestaDto<bool>.Fallo(
                "Ese correo o teléfono no está registrado.",
                404
            );
        }

        // GENERAR NUEVA CONTRASEÑA
        var nuevaPassword =
            "Rotter" + Random.Shared.Next(1000, 9999) + "!";

        // ENCRIPTAR CONTRASEÑA
        usuario.PasswordHash =
            BCrypt.Net.BCrypt.HashPassword(nuevaPassword);

        // GUARDAR EN BASE DE DATOS
        await _usuarios.ActualizarAsync(usuario);

        // ENVIAR CORREO
        await _email.EnviarAsync(
            usuario.Email,
            "Recuperación de contraseña",
            $@"
        <div style='
            font-family: Arial;
            padding: 20px;
            background: #f5f5f5;
        '>

            <div style='
                max-width: 500px;
                margin: auto;
                background: white;
                border-radius: 12px;
                padding: 30px;
                box-shadow: 0 2px 10px rgba(0,0,0,.08);
            '>

                <h2 style='color:#1a6b8a;'>
                    Recuperación de contraseña
                </h2>

                <p>
                    Hola <strong>{usuario.Nombre}</strong>,
                </p>

                <p>
                    Tu nueva contraseña es:
                </p>

                <div style='
                    background:#eef7fa;
                    border:1px solid #cce5ee;
                    padding:20px;
                    border-radius:10px;
                    text-align:center;
                    margin:20px 0;
                '>

                    <h1 style='
                        margin:0;
                        color:#0f5470;
                        letter-spacing:2px;
                    '>
                        {nuevaPassword}
                    </h1>

                </div>

                <p style='color:#666; font-size:14px;'>
                    Ingresa al sistema y cambia tu contraseña.
                </p>

            </div>

        </div>
        "
        );

        return RespuestaDto<bool>.Ok(
            true,
            "Correo enviado correctamente."
        );
    }
}