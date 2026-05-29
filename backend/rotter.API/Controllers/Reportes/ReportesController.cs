using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Reportes.Queries;

namespace rotter.API.Controllers.Reportes;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class ReportesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReportesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("pdf-mensual")]
    public async Task<IActionResult> PdfMensual(
    [FromQuery(Name = "anio")] int anio,
    [FromQuery(Name = "mes")] int mes)
    {
        Console.WriteLine($"ANIO: {anio}");
        Console.WriteLine($"MES: {mes}");

        var r = await _mediator.Send(
            new GenerarPdfMensualQuery(anio, mes));

        if (!r.Exito)
            return BadRequest(r);

        return File(
            r.Datos!,
            "application/pdf",
            $"ventas-{anio}-{mes:D2}.pdf");
    }

    [HttpGet("pdf-colaborador")]
    public async Task<IActionResult> PdfColaborador([FromQuery] int colaboradorId, [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var r = await _mediator.Send(new GenerarPdfColaboradorQuery(colaboradorId, desde, hasta));
        if (!r.Exito) return BadRequest(r);
        return File(r.Datos!, "application/pdf", $"colaborador-{colaboradorId}-{desde:yyyyMMdd}.pdf");
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var r = await _mediator.Send(new GenerarExcelQuery(desde, hasta));
        if (!r.Exito) return BadRequest(r);
        return File(r.Datos!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ventas-{desde:yyyyMMdd}.xlsx");
    }
}
