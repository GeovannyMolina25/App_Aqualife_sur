using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Cotizaciones.Commands;
using rotter.Aplicacion.Cotizaciones.Queries;
using rotter.Dominio.DTOs.Cotizaciones;

namespace rotter.API.Controllers.Cotizaciones;

[ApiController]
[Route("api/[controller]")]
public class CotizacionesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CotizacionesController(IMediator mediator) => _mediator = mediator;

    /// <summary>Solicitar cotización de un servicio. No requiere sesión (permite invitados);
    /// si la petición llega autenticada, la solicitud queda asociada al cliente.</summary>
    [HttpPost]
    public async Task<IActionResult> Solicitar([FromBody] CrearCotizacionServicioDto dto)
    {
        int? clienteId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (claim is not null) clienteId = int.Parse(claim);
        }

        var r = await _mediator.Send(new SolicitarCotizacionCommand(dto, clienteId));
        return StatusCode(r.StatusCode, r);
    }

    /// <summary>Listado para el personal (nunca para clientes): permite ver quién pidió un
    /// servicio para poder contactarlo, filtrando por estado de gestión.</summary>
    [HttpGet]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> ObtenerTodas([FromQuery] int pagina = 1, [FromQuery] int tamano = 20,
        [FromQuery] string? estado = null, [FromQuery] string? busqueda = null)
        => Ok(await _mediator.Send(new ObtenerCotizacionesQuery(pagina, tamano, estado, busqueda)));

    [HttpPut("{id}/estado")]
    [Authorize(Roles = "Administrador,Colaborador,SuperAdministrador")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoCotizacionDto dto)
    {
        var r = await _mediator.Send(new CambiarEstadoCotizacionCommand(id, dto.Estado));
        return StatusCode(r.StatusCode, r);
    }
}
