using MediatR;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Productos.Queries;

namespace rotter.API.Controllers.Productos;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoriasController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
        => Ok(await _mediator.Send(new ObtenerCategoriasQuery()));
}
