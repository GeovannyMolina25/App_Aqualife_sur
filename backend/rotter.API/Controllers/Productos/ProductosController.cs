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
    public async Task<IActionResult> ObtenerTodos([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, [FromQuery] string? busqueda = null,
        [FromQuery] int? categoriaId = null, [FromQuery] string? tipo = null)
        => Ok(await _mediator.Send(new ObtenerProductosQuery(pagina, tamano, Busqueda: busqueda, CategoriaId: categoriaId, Tipo: tipo)));

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

    [HttpPost("con-imagen")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> CrearConImagen([FromForm] CrearProductoConImagenForm form)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Stream? contenido = null;
        try
        {
            if (form.Imagen is not null) contenido = form.Imagen.OpenReadStream();
            var r = await _mediator.Send(new CrearProductoConImagenCommand(
                form.Nombre, form.Descripcion, form.CategoriaId, form.Precio, form.Stock, usuarioId,
                contenido, form.Imagen?.FileName, form.Imagen?.ContentType, form.Imagen?.Length ?? 0));
            return StatusCode(r.StatusCode, r);
        }
        finally
        {
            if (contenido is not null) await contenido.DisposeAsync();
        }
    }
}

public class CrearProductoConImagenForm
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CategoriaId { get; set; }
    public decimal Precio { get; set; } = 0;
    public int Stock { get; set; } = 0;
    public IFormFile? Imagen { get; set; }
}
