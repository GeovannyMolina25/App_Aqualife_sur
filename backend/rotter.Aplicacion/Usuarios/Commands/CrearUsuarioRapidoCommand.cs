using System.Globalization;
using System.Text;
using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Usuarios;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Usuarios.Commands;

/// <summary>
/// Alta rápida de clientes desde el panel. Lo que no se envía se completa con valores por defecto:
/// correo nombreapellidoNN@acqualife.com, teléfono 0987654321 y contraseña 123456.
/// </summary>
public record CrearUsuarioRapidoCommand(CrearUsuarioRapidoDto Dto) : IRequest<RespuestaDto<UsuarioRapidoCreadoDto>>;

public class CrearUsuarioRapidoHandler : IRequestHandler<CrearUsuarioRapidoCommand, RespuestaDto<UsuarioRapidoCreadoDto>>
{
    public const string PasswordPorDefecto = "123456";
    public const string TelefonoPorDefecto = "0987654321";
    private const string DominioPorDefecto = "acqualife.com";
    private const string SexoPorDefecto    = "Otro"; // CHECK de la tabla: Masculino | Femenino | Otro
    private const int RolCliente = 3;

    private readonly IUsuarioRepositorio _usuarios;
    private readonly IAuditoriaServicio  _auditoria;

    public CrearUsuarioRapidoHandler(IUsuarioRepositorio usuarios, IAuditoriaServicio auditoria)
    { _usuarios = usuarios; _auditoria = auditoria; }

    public async Task<RespuestaDto<UsuarioRapidoCreadoDto>> Handle(CrearUsuarioRapidoCommand req, CancellationToken ct)
    {
        var nombre   = req.Dto.Nombre.Trim();
        var apellido = req.Dto.Apellido.Trim();

        string email;
        if (!string.IsNullOrWhiteSpace(req.Dto.Email))
        {
            email = req.Dto.Email.Trim().ToLower();
            if (await _usuarios.ExisteEmailAsync(email))
                return RespuestaDto<UsuarioRapidoCreadoDto>.Fallo("El email ya está registrado.");
        }
        else
        {
            email = await GenerarEmailAsync(nombre, apellido);
        }

        var telefono = string.IsNullOrWhiteSpace(req.Dto.Telefono) ? TelefonoPorDefecto : req.Dto.Telefono.Trim();

        var usuario = new Usuario
        {
            Nombre = nombre, Apellido = apellido, Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordPorDefecto),
            Sexo      = SexoPorDefecto,
            Direccion = req.Dto.Direccion?.Trim() ?? string.Empty,
            Telefono  = telefono,
            RolId     = RolCliente,
            RequiereCambioPassword = true
        };

        await _usuarios.CrearAsync(usuario);
        await _auditoria.RegistrarAsync("REGISTRO_RAPIDO", "Usuarios", usuario.Id.ToString(),
            datosNuevos: new { usuario.Email, usuario.Nombre, usuario.Apellido });

        return RespuestaDto<UsuarioRapidoCreadoDto>.Ok(
            new UsuarioRapidoCreadoDto(usuario.Id, usuario.Nombre, usuario.Apellido, usuario.Email, telefono),
            "Usuario creado correctamente.");
    }

    /// <summary>nombre + apellido (sin tildes ni espacios) + dos dígitos al azar, garantizando que no exista.</summary>
    private async Task<string> GenerarEmailAsync(string nombre, string apellido)
    {
        var baseLocal = Normalizar(PrimeraPalabra(nombre)) + Normalizar(PrimeraPalabra(apellido));
        if (baseLocal.Length == 0) baseLocal = "cliente";

        for (var intento = 0; intento < 30; intento++)
        {
            var sufijo = intento < 20 ? Random.Shared.Next(10, 100) : Random.Shared.Next(100, 10000);
            var email = $"{baseLocal}{sufijo}@{DominioPorDefecto}";
            if (!await _usuarios.ExisteEmailAsync(email)) return email;
        }
        return $"{baseLocal}{Guid.NewGuid().ToString("N")[..8]}@{DominioPorDefecto}";
    }

    private static string PrimeraPalabra(string s) =>
        s.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    private static string Normalizar(string s)
    {
        var sb = new StringBuilder();
        foreach (var c in s.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsAsciiLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
