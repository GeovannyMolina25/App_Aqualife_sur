using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rotter.Aplicacion.Reportes.Queries;

namespace rotter.API.Controllers.Reportes;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,SuperAdministrador")]
public class ReportesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReportesController> _logger;
    public ReportesController(IMediator mediator, ILogger<ReportesController> logger)
    { _mediator = mediator; _logger = logger; }

    [HttpGet("pdf-mensual")]
    public async Task<IActionResult> PdfMensual(
    [FromQuery(Name = "anio")] int anio,
    [FromQuery(Name = "mes")] int mes)
    {
        _logger.LogInformation("Generando reporte PDF mensual {Anio}-{Mes}", anio, mes);

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

    [HttpGet("ranking")]
    public async Task<IActionResult> Ranking([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string formato = "pdf")
    {
        var esExcel = formato.Equals("excel", StringComparison.OrdinalIgnoreCase);
        var r = await _mediator.Send(new GenerarRankingQuery(desde, hasta, esExcel ? FormatoReporte.Excel : FormatoReporte.Pdf));
        if (!r.Exito) return BadRequest(r);

        var nombre = $"ranking-ventas-{DateTime.Now:yyyyMMdd}";
        return esExcel
            ? File(r.Datos!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{nombre}.xlsx")
            : File(r.Datos!, "application/pdf", $"{nombre}.pdf");
    }

    [HttpGet("excel")]
    public async Task<IActionResult> Excel([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var r = await _mediator.Send(new GenerarExcelQuery(desde, hasta));
        if (!r.Exito) return BadRequest(r);
        return File(r.Datos!, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ventas-{desde:yyyyMMdd}.xlsx");
    }
}
