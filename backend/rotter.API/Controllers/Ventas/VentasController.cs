using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Ventas.Commands;
using rotter.Aplicacion.Ventas.Queries;
using rotter.Dominio.DTOs.Ventas;

namespace rotter.API.Controllers.Ventas;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IMediator _mediator;
    public VentasController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> ObtenerTodas([FromQuery] int pagina = 1, [FromQuery] int tamano = 20,
        [FromQuery] DateTime? desde = null, [FromQuery] DateTime? hasta = null, [FromQuery] string? busqueda = null)
        => Ok(await _mediator.Send(new ObtenerVentasQuery(pagina, tamano, desde, hasta, busqueda)));

    [HttpGet("metricas")]
    [Authorize(Roles = "Administrador,SuperAdministrador")]
    public async Task<IActionResult> Metricas()
        => Ok(await _mediator.Send(new ObtenerMetricasQuery()));

    [HttpGet("mi-historial")]
    public async Task<IActionResult> MiHistorial([FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        return Ok(await _mediator.Send(new ObtenerHistorialClienteQuery(clienteId, pagina, tamano)));
    }
    [HttpGet("{id}/factura")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> ObtenerFactura(int id)
    {
        var resultado =
            await _mediator.Send(
                new ObtenerFacturaQuery(id)
            );

        return StatusCode(
            resultado.StatusCode,
            resultado
        );
    }
    [HttpGet("{id}/factura-pdf")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> DescargarFacturaPdf(int id)
    {
        var pdf =
            await _mediator.Send(
                new DescargarFacturaPdfQuery(id));

        return File(
            pdf,
            "application/pdf",
            $"Factura_{id}.pdf");
    }
    [HttpPost]
    [Authorize(Roles = "Administrador,Colaborador,Cliente,SuperAdministrador")]
    public async Task<IActionResult> Registrar([FromBody] CrearVentaDto dto)
    {
        var colaboradorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new RegistrarVentaCommand(dto, colaboradorId));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("checkout")]
    [Authorize(Roles = "Cliente,Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutDto dto)
    {
        var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new RegistrarPedidoWebCommand(dto, clienteId));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("{id}/comprobante")]
    [Authorize(Roles = "Cliente,Administrador,Colaborador,SuperAdministrador")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> SubirComprobante(int id, IFormFile archivo)
    {
        var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var esStaff = User.IsInRole("Administrador") || User.IsInRole("Colaborador") || User.IsInRole("SuperAdministrador");

        await using var stream = archivo.OpenReadStream();
        var r = await _mediator.Send(new SubirComprobanteCommand(
            id, clienteId, esStaff, stream, archivo.FileName, archivo.ContentType, archivo.Length));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPut("{id}/estado-pago")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> ActualizarEstadoPago(int id, [FromBody] ActualizarEstadoPagoDto dto)
    {
        var r = await _mediator.Send(new ActualizarEstadoPagoCommand(id, dto.NuevoEstadoPago));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPut("{id}/estado")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoVentaDto dto)
    {
        var r = await _mediator.Send(new CambiarEstadoVentaCommand(id, dto.NuevoEstado));
        return StatusCode(r.StatusCode, r);
    }
}

public record ActualizarEstadoPagoDto(string NuevoEstadoPago);
