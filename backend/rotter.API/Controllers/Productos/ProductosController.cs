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
        [FromQuery] int? categoriaId = null, [FromQuery] string? tipo = null, [FromQuery] bool incluirInactivos = false)
    {
        // Solo un administrador puede ver los ítems ocultos (para volver a publicarlos).
        var soloActivos = !(incluirInactivos && EsAdministrador());
        return Ok(await _mediator.Send(new ObtenerProductosQuery(pagina, tamano, soloActivos, busqueda, categoriaId, tipo)));
    }

    private bool EsAdministrador() => User.IsInRole("Administrador") || User.IsInRole("SuperAdministrador");

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
                contenido, form.Imagen?.FileName, form.Imagen?.ContentType, form.Imagen?.Length ?? 0,
                form.Caracteristicas, form.EsPromocion, form.PrecioPromocion));
            return StatusCode(r.StatusCode, r);
        }
        finally
        {
            if (contenido is not null) await contenido.DisposeAsync();
        }
    }

    [HttpPut("{id:int}/con-imagen")]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> ActualizarConImagen(int id, [FromForm] CrearProductoConImagenForm form)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Stream? contenido = null;
        try
        {
            if (form.Imagen is not null) contenido = form.Imagen.OpenReadStream();
            var r = await _mediator.Send(new ActualizarProductoConImagenCommand(
                id, form.Nombre, form.Descripcion, form.Caracteristicas, form.CategoriaId, form.Precio,
                form.EsPromocion, form.PrecioPromocion, usuarioId,
                contenido, form.Imagen?.FileName, form.Imagen?.ContentType, form.Imagen?.Length ?? 0));
            return StatusCode(r.StatusCode, r);
        }
        finally
        {
            if (contenido is not null) await contenido.DisposeAsync();
        }
    }

    [HttpPut("{id:int}/estado")]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoProductoDto dto)
    {
        var r = await _mediator.Send(new CambiarEstadoProductoCommand(id, dto.Activo));
        return StatusCode(r.StatusCode, r);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await _mediator.Send(new EliminarProductoCommand(id));
        return StatusCode(r.StatusCode, r);
    }
}

public record CambiarEstadoProductoDto(bool Activo);

public class CrearProductoConImagenForm
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Caracteristicas { get; set; }
    public int CategoriaId { get; set; }
    public decimal Precio { get; set; } = 0;
    public int Stock { get; set; } = 0;
    public bool EsPromocion { get; set; }
    public decimal? PrecioPromocion { get; set; }
    public IFormFile? Imagen { get; set; }
}
