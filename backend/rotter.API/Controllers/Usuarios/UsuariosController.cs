using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Usuarios.Commands;
using rotter.Aplicacion.Usuarios.Queries;
using rotter.Dominio.DTOs.Usuarios;

namespace rotter.API.Controllers.Usuarios;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsuariosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Administrador,Colaborador")]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var r = await _mediator.Send(new ObtenerUsuariosQuery(pagina, tamano));
        return Ok(r);
    }

    [HttpPut("{id}/rol")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CambiarRol(int id, [FromBody] CambiarRolDto dto)
    {
        var r = await _mediator.Send(new CambiarRolCommand(id, dto.NuevoRolId));
        return StatusCode(r.StatusCode, r);
    }
}
