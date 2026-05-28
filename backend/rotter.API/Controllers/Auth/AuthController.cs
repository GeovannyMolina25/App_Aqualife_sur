using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
    {
        var r = await _mediator.Send(new RegistrarUsuarioCommand(dto));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var r = await _mediator.Send(new LoginCommand(dto));
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
}
