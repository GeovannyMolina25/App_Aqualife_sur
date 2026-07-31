using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Productos.Commands;
using rotter.Aplicacion.Productos.Queries;
using rotter.Dominio.DTOs.Productos;

namespace rotter.API.Controllers.Productos;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductosController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, [FromQuery] string? busqueda = null)
        => Ok(await _mediator.Send(new ObtenerProductosQuery(pagina, tamano, Busqueda: busqueda)));

    [HttpGet("promociones")]
    public async Task<IActionResult> ObtenerPromociones()
        => Ok(await _mediator.Send(new ObtenerPromocionesQuery()));

    [HttpPost]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> Crear([FromBody] CrearProductoDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new CrearProductoCommand(dto, usuarioId));
        return StatusCode(r.StatusCode, r);
    }
}
