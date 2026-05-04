using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Reportes;

public class ReporteServicio : IReporteServicio
{
    private const string Azul   = "#1a6b8a";
    private const string Blanco = "#FFFFFF";

    public byte[] GenerarPdfVentasMensuales(List<Venta> ventas, int año, int mes)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var nombreMes = new System.Globalization.CultureInfo("es-ES").DateTimeFormat.GetMonthName(mes);
        var total = ventas.Sum(v => v.Total);

        return Document.Create(c => c.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(40);
            page.Header().Column(col =>
            {
                col.Item().Text($"ROTTER — Ventas {nombreMes} {año}").FontSize(20).Bold().FontColor(Azul);
                col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#666");
                col.Item().LineHorizontal(1).LineColor(Azul);
            });
            page.Content().PaddingTop(16).Column(col =>
            {
                col.Item().Text($"Total: ${total:N2}  ·  Ventas: {ventas.Count}").FontSize(13).Bold();
                col.Item().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); });
                    t.Header(h => { foreach (var th in new[] { "# Venta","Cliente","Colaborador","Fecha","Total" }) h.Cell().Background(Azul).Padding(5).Text(th).FontColor(Blanco).Bold().FontSize(9); });
                    foreach (var v in ventas)
                    {
                        t.Cell().Padding(5).Text(v.NumeroVenta).FontSize(8);
                        t.Cell().Padding(5).Text($"{v.Cliente?.Nombre} {v.Cliente?.Apellido}").FontSize(8);
                        t.Cell().Padding(5).Text($"{v.Colaborador?.Nombre} {v.Colaborador?.Apellido}").FontSize(8);
                        t.Cell().Padding(5).Text(v.FechaVenta.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                        t.Cell().Padding(5).Text($"${v.Total:N2}").FontSize(8);
                    }
                });
            });
            page.Footer().AlignRight().Text(x => { x.Span("Pág. "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
        })).GeneratePdf();
    }

    public byte[] GenerarPdfVentasColaborador(List<Venta> ventas, string nombreColaborador, DateTime desde, DateTime hasta)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var total = ventas.Sum(v => v.Total);

        return Document.Create(c => c.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(40);
            page.Header().Column(col =>
            {
                col.Item().Text($"Ventas: {nombreColaborador}").FontSize(18).Bold().FontColor(Azul);
                col.Item().Text($"Período: {desde:dd/MM/yyyy} — {hasta:dd/MM/yyyy}").FontSize(11);
                col.Item().LineHorizontal(1).LineColor(Azul);
            });
            page.Content().PaddingTop(16).Column(col =>
            {
                col.Item().Text($"Total: ${total:N2}  ·  Ventas: {ventas.Count}").FontSize(13).Bold();
                col.Item().PaddingTop(10).Table(t =>
                {
                    t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(3); c.RelativeColumn(2); c.RelativeColumn(2); });
                    t.Header(h => { foreach (var th in new[] { "# Venta","Cliente","Fecha","Total" }) h.Cell().Background(Azul).Padding(5).Text(th).FontColor(Blanco).Bold().FontSize(9); });
                    foreach (var v in ventas)
                    {
                        t.Cell().Padding(5).Text(v.NumeroVenta).FontSize(8);
                        t.Cell().Padding(5).Text($"{v.Cliente?.Nombre} {v.Cliente?.Apellido}").FontSize(8);
                        t.Cell().Padding(5).Text(v.FechaVenta.ToString("dd/MM/yyyy HH:mm")).FontSize(8);
                        t.Cell().Padding(5).Text($"${v.Total:N2}").FontSize(8);
                    }
                });
            });
        })).GeneratePdf();
    }

    public byte[] GenerarExcelProductosVendidos(List<DetalleVenta> detalles)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Productos Vendidos");
        var headers = new[] { "Producto","Características","Precio Unit.","Cantidad","Subtotal","Cliente","Colaborador","Fecha","Hora" };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.FromHtml(Azul);
            ws.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var d in detalles)
        {
            ws.Cell(row, 1).Value = d.Producto?.Nombre ?? "";
            ws.Cell(row, 2).Value = d.Producto?.Caracteristicas ?? "";
            ws.Cell(row, 3).Value = d.PrecioUnitario;
            ws.Cell(row, 4).Value = d.Cantidad;
            ws.Cell(row, 5).Value = d.Subtotal;
            ws.Cell(row, 6).Value = $"{d.Venta?.Cliente?.Nombre} {d.Venta?.Cliente?.Apellido}";
            ws.Cell(row, 7).Value = $"{d.Venta?.Colaborador?.Nombre} {d.Venta?.Colaborador?.Apellido}";
            ws.Cell(row, 8).Value = d.Venta?.FechaVenta.ToString("dd/MM/yyyy");
            ws.Cell(row, 9).Value = d.Venta?.FechaVenta.ToString("HH:mm:ss");
            row++;
        }

        ws.Columns().AdjustToContents();
        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }
}
