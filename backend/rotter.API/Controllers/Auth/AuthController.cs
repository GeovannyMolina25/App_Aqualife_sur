using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using rotter.Aplicacion.Auth.Commands;
using rotter.Dominio.DTOs.Auth;

namespace rotter.API.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("registro")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
    {
        var r = await _mediator.Send(new RegistrarUsuarioCommand(dto));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var r = await _mediator.Send(new LoginCommand(dto));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Refrescar([FromBody] RefrescarTokenDto dto)
    {
        var r = await _mediator.Send(new RefrescarTokenCommand(dto.RefreshToken));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new CerrarSesionCommand(usuarioId));
        return StatusCode(r.StatusCode, r);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        id     = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        email  = User.FindFirst(ClaimTypes.Email)?.Value,
        nombre = User.FindFirst(ClaimTypes.Name)?.Value,
        rol    = User.FindFirst(ClaimTypes.Role)?.Value
    });
    [HttpPost("recuperar-password")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RecuperarPassword(
    [FromBody] OlvidePasswordDto dto
)
    {
        var r = await _mediator.Send(
            new RecuperarPasswordCommand(dto.Dato)
        );

        return StatusCode(r.StatusCode, r);
    }
    [HttpPut("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordDto dto)
    {
        var usuarioId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value
        );
        var r = await _mediator.Send(
            new CambiarPasswordCommand(
                usuarioId,
                dto.NuevaPassword
            )
        );
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("2fa/iniciar")]
    [Authorize]
    public async Task<IActionResult> Iniciar2FA()
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new IniciarDosFactoresCommand(usuarioId));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("2fa/confirmar")]
    [Authorize]
    public async Task<IActionResult> Confirmar2FA([FromBody] ConfirmarDosFactoresDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new ConfirmarDosFactoresCommand(usuarioId, dto.Codigo));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("2fa/desactivar")]
    [Authorize]
    public async Task<IActionResult> Desactivar2FA([FromBody] DesactivarDosFactoresDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new DesactivarDosFactoresCommand(usuarioId, dto.Codigo));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("2fa/verificar")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Verificar2FA([FromBody] VerificarDosFactoresDto dto)
    {
        var r = await _mediator.Send(new VerificarDosFactoresCommand(dto.TokenDesafio, dto.Codigo));
        return StatusCode(r.StatusCode, r);
    }
}
