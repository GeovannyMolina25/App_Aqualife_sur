using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Caja.Commands;
using rotter.Aplicacion.Caja.Queries;
using rotter.Dominio.DTOs.Caja;

namespace rotter.API.Controllers.Caja;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CajaController : ControllerBase
{
    private readonly IMediator _mediator;
    public CajaController(IMediator mediator) => _mediator = mediator;

    [HttpGet("saldo")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> ObtenerSaldo()
        => Ok(await _mediator.Send(new ObtenerSaldoCajaQuery()));

    [HttpGet("historial")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> ObtenerHistorial([FromQuery] int pagina = 1, [FromQuery] int tamano = 20, [FromQuery] string? busqueda = null)
        => Ok(await _mediator.Send(new ObtenerHistorialCajaQuery(pagina, tamano, busqueda)));

    [HttpGet("ventas-pendientes")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> ObtenerVentasPendientes()
        => Ok(await _mediator.Send(new ObtenerVentasPendientesQuery()));

    [HttpPost("ingresos")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> RegistrarIngreso([FromBody] CrearIngresoCajaDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new RegistrarIngresoCajaCommand(dto, usuarioId));
        return StatusCode(r.StatusCode, r);
    }

    [HttpPost("salidas")]
    [Authorize(Roles = "SuperAdministrador")]
    public async Task<IActionResult> RegistrarSalida([FromBody] CrearSalidaCajaDto dto)
    {
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var r = await _mediator.Send(new RegistrarSalidaCajaCommand(dto, usuarioId));
        return StatusCode(r.StatusCode, r);
    }
}
