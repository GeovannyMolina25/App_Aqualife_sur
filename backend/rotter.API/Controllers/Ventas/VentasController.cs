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

    [HttpPut("{id}/estado")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoVentaDto dto)
    {
        var r = await _mediator.Send(new CambiarEstadoVentaCommand(id, dto.NuevoEstado));
        return StatusCode(r.StatusCode, r);
    }
}
