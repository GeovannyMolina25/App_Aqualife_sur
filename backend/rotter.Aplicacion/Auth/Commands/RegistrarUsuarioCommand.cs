using BCrypt.Net;
using MediatR;
using rotter.Dominio.DTOs.Auth;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Auth.Commands;

public record RegistrarUsuarioCommand(RegistroDto Dto) : IRequest<RespuestaDto<AuthResponseDto>>;

public class RegistrarUsuarioHandler : IRequestHandler<RegistrarUsuarioCommand, RespuestaDto<AuthResponseDto>>
{
    private readonly IUsuarioRepositorio _usuarios;
    private readonly IJwtServicio        _jwt;
    private readonly IAuditoriaServicio  _auditoria;

    public RegistrarUsuarioHandler(IUsuarioRepositorio usuarios, IJwtServicio jwt, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _jwt = jwt; _auditoria = auditoria; }

    public async Task<RespuestaDto<AuthResponseDto>> Handle(RegistrarUsuarioCommand req, CancellationToken ct)
    {
        if (await _usuarios.ExisteEmailAsync(req.Dto.Email))
            return RespuestaDto<AuthResponseDto>.Fallo("El email ya está registrado.");

        var usuario = new Usuario
        {
            Nombre = req.Dto.Nombre, Apellido = req.Dto.Apellido,
            Email  = req.Dto.Email.ToLower(),
            PasswordHash    = BCrypt.Net.BCrypt.HashPassword(req.Dto.Password),
            FechaNacimiento = req.Dto.FechaNacimiento,
            Sexo     = req.Dto.Sexo, Direccion = req.Dto.Direccion,
            Telefono = req.Dto.Telefono, RolId = 3
        };

        await _usuarios.CrearAsync(usuario);
        var completo = await _usuarios.ObtenerPorIdAsync(usuario.Id);
        var token    = _jwt.GenerarToken(completo!);

        await _auditoria.RegistrarAsync("REGISTRO", "Usuarios", usuario.Id.ToString(),
            datosNuevos: new { usuario.Email, usuario.Nombre });

        return RespuestaDto<AuthResponseDto>.Ok(new AuthResponseDto(
            token, _jwt.GenerarRefreshToken(),
            new UsuarioAuthDto(completo!.Id, completo.Nombre, completo.Apellido, completo.Email, completo.Rol.Nombre),
            DateTime.UtcNow.AddHours(24)), "Registro exitoso.");
    }
}
