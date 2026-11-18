using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Auditoria.Queries;

namespace rotter.API.Controllers.Auditoria;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdministrador")]
public class AuditoriaController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditoriaController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Obtener([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, [FromQuery] string? accion = null)
    {
        var r = await _mediator.Send(new ObtenerAuditoriaQuery(pagina, tamano, accion));
        return Ok(r);
    }
}
