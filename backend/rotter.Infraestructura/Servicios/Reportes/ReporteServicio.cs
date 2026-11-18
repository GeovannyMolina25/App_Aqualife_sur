using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using rotter.Dominio.Constantes;
using rotter.Dominio.DTOs.Reportes;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;
using System.Globalization;

namespace rotter.Infraestructura.Servicios.Reportes;

public class ReporteServicio : IReporteServicio
{
    private const string Azul   = "#1a6b8a";
    private const string Blanco = "#FFFFFF";

    public byte[] GenerarPdfVentasMensuales(List<Venta> ventas, int anio, int mes)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var nombreMes = new CultureInfo("es-ES")
        .DateTimeFormat
        .GetMonthName(mes);
        var total = ventas.Sum(v => v.Total);

        return Document.Create(c => c.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(40);
            page.Header().Column(col =>
            {
                col.Item().Text($"ROTTER — Ventas {nombreMes} {anio}").FontSize(20).Bold().FontColor(Azul);
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

    // ================= Ranking de ventas (clientes, productos, servicios) =================

    private const string GrisTexto = "#7a9aaa";
    private const string FilaAlterna = "#f8f3ea";

    private static string TextoPeriodo(RankingReporteDto d) => (d.Desde, d.Hasta) switch
    {
        (null, null) => "Todo el historial",
        ({ } desde, null) => $"Desde {desde:dd/MM/yyyy}",
        (null, { } hasta) => $"Hasta {hasta:dd/MM/yyyy}",
        ({ } desde, { } hasta) => $"{desde:dd/MM/yyyy} — {hasta:dd/MM/yyyy}",
    };

    public byte[] GenerarPdfRanking(RankingReporteDto datos)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(c => c.Page(page =>
        {
            page.Size(PageSizes.A4); page.Margin(36);
            page.DefaultTextStyle(x => x.FontSize(9));

            page.Header().Column(col =>
            {
                col.Item().Text($"{EmpresaConstantes.Nombre} — Ranking de Ventas").FontSize(20).Bold().FontColor(Azul);
                col.Item().Text($"Período: {TextoPeriodo(datos)}  ·  Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor("#666");
                col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Azul);
            });

            page.Content().PaddingTop(14).Column(col =>
            {
                col.Spacing(18);

                col.Item().Row(row =>
                {
                    row.Spacing(10);
                    Resumen(row.RelativeItem(), "Clientes con compras", datos.Clientes.Count.ToString());
                    Resumen(row.RelativeItem(), "Total gastado por clientes", $"${datos.Clientes.Sum(x => x.TotalGastado):N2}");
                    Resumen(row.RelativeItem(), "Productos vendidos", $"{datos.Productos.Sum(x => x.Unidades)} u.");
                    Resumen(row.RelativeItem(), "Servicios vendidos", $"{datos.Servicios.Sum(x => x.Unidades)}");
                });

                col.Item().Column(sec =>
                {
                    Titulo(sec, "Clientes que más compraron", "Ordenados por total gastado");
                    sec.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.ConstantColumn(26); cd.RelativeColumn(3); cd.RelativeColumn(3); cd.RelativeColumn(1.2f); cd.RelativeColumn(1.3f); cd.RelativeColumn(1.6f); cd.RelativeColumn(1.6f); });
                        Encabezado(t, "#", "Cliente", "Correo", "Compras", "Productos", "Última compra", "Total gastado");
                        if (datos.Clientes.Count == 0) SinDatos(t, 7, "No hay compras en este período.");
                        for (var i = 0; i < datos.Clientes.Count; i++)
                        {
                            var x = datos.Clientes[i];
                            var fondo = i % 2 == 1 ? FilaAlterna : Blanco;
                            Celda(t, fondo, (i + 1).ToString());
                            Celda(t, fondo, x.Nombre);
                            Celda(t, fondo, x.Email);
                            Celda(t, fondo, x.Compras.ToString(), derecha: true);
                            Celda(t, fondo, x.Unidades.ToString(), derecha: true);
                            Celda(t, fondo, x.UltimaCompra?.ToString("dd/MM/yyyy") ?? "—");
                            Celda(t, fondo, $"${x.TotalGastado:N2}", derecha: true, negrita: true);
                        }
                    });
                });

                col.Item().Column(sec => TablaItems(sec, "Productos más vendidos", "Unidades", datos.Productos));
                col.Item().Column(sec => TablaItems(sec, "Servicios más vendidos", "Veces", datos.Servicios));
            });

            page.Footer().AlignRight().Text(x => { x.Span("Pág. "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages(); });
        })).GeneratePdf();
    }

    private static void TablaItems(ColumnDescriptor sec, string titulo, string unidad, List<TopProductoData> items)
    {
        Titulo(sec, titulo, $"Ordenados por {unidad.ToLower()} · los que no se vendieron aparecen al final");
        sec.Item().Table(t =>
        {
            t.ColumnsDefinition(cd => { cd.ConstantColumn(26); cd.RelativeColumn(4); cd.RelativeColumn(2.5f); cd.RelativeColumn(1.3f); cd.RelativeColumn(1.6f); });
            Encabezado(t, "#", "Nombre", "Categoría", unidad, "Total vendido");
            if (items.Count == 0) SinDatos(t, 5, "No hay registros.");
            for (var i = 0; i < items.Count; i++)
            {
                var x = items[i];
                var fondo = i % 2 == 1 ? FilaAlterna : Blanco;
                var apagado = x.Unidades == 0;
                Celda(t, fondo, (i + 1).ToString(), apagado: apagado);
                Celda(t, fondo, x.Nombre, apagado: apagado);
                Celda(t, fondo, x.Categoria, apagado: apagado);
                Celda(t, fondo, x.Unidades.ToString(), derecha: true, apagado: apagado);
                Celda(t, fondo, $"${x.MontoTotal:N2}", derecha: true, negrita: !apagado, apagado: apagado);
            }
        });
    }

    private static void Resumen(IContainer c, string etiqueta, string valor) =>
        c.Border(1).BorderColor("#efe4cc").Background(Blanco).Padding(10).Column(col =>
        {
            col.Item().Text(etiqueta.ToUpper()).FontSize(7).Bold().FontColor(GrisTexto);
            col.Item().PaddingTop(3).Text(valor).FontSize(14).Bold().FontColor(Azul);
        });

    private static void Titulo(ColumnDescriptor sec, string titulo, string subtitulo)
    {
        sec.Item().Text(titulo).FontSize(13).Bold().FontColor("#1c2b35");
        sec.Item().PaddingBottom(6).Text(subtitulo).FontSize(8).FontColor(GrisTexto);
    }

    private static void Encabezado(TableDescriptor t, params string[] columnas) =>
        t.Header(h => { foreach (var th in columnas) h.Cell().Background(Azul).Padding(5).Text(th).FontColor(Blanco).Bold().FontSize(8); });

    private static void SinDatos(TableDescriptor t, int columnas, string mensaje) =>
        t.Cell().ColumnSpan((uint)columnas).Padding(10).AlignCenter().Text(mensaje).FontColor(GrisTexto);

    private static void Celda(TableDescriptor t, string fondo, string texto, bool derecha = false, bool negrita = false, bool apagado = false)
    {
        var c = t.Cell().Background(fondo).PaddingVertical(4).PaddingHorizontal(5);
        if (derecha) c = c.AlignRight();
        var txt = c.Text(texto).FontSize(8).FontColor(apagado ? "#a9bcc6" : "#1c2b35");
        if (negrita) txt.Bold();
    }

    public byte[] GenerarExcelRanking(RankingReporteDto datos)
    {
        using var wb = new XLWorkbook();
        var periodo = TextoPeriodo(datos);

        var wsC = wb.Worksheets.Add("Clientes");
        var filaC = EncabezadoExcel(wsC, $"Clientes que más compraron · {periodo}",
            "#", "Cliente", "Correo", "Compras", "Productos", "Última compra", "Total gastado");
        foreach (var (x, i) in datos.Clientes.Select((x, i) => (x, i)))
        {
            wsC.Cell(filaC, 1).Value = i + 1;
            wsC.Cell(filaC, 2).Value = x.Nombre;
            wsC.Cell(filaC, 3).Value = x.Email;
            wsC.Cell(filaC, 4).Value = x.Compras;
            wsC.Cell(filaC, 5).Value = x.Unidades;
            if (x.UltimaCompra is { } u) { wsC.Cell(filaC, 6).Value = u; wsC.Cell(filaC, 6).Style.DateFormat.Format = "dd/MM/yyyy"; }
            wsC.Cell(filaC, 7).Value = x.TotalGastado;
            filaC++;
        }
        TotalesExcel(wsC, filaC, 3, new[] { 4, 5, 7 }, montoCol: 7);

        HojaItemsExcel(wb, "Productos", $"Productos más vendidos · {periodo}", "Unidades", datos.Productos);
        HojaItemsExcel(wb, "Servicios", $"Servicios más vendidos · {periodo}", "Veces", datos.Servicios);

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static void HojaItemsExcel(XLWorkbook wb, string hoja, string titulo, string unidad, List<TopProductoData> items)
    {
        var ws = wb.Worksheets.Add(hoja);
        var fila = EncabezadoExcel(ws, titulo, "#", "Nombre", "Categoría", unidad, "Total vendido");
        foreach (var (x, i) in items.Select((x, i) => (x, i)))
        {
            ws.Cell(fila, 1).Value = i + 1;
            ws.Cell(fila, 2).Value = x.Nombre;
            ws.Cell(fila, 3).Value = x.Categoria;
            ws.Cell(fila, 4).Value = x.Unidades;
            ws.Cell(fila, 5).Value = x.MontoTotal;
            if (x.Unidades == 0) ws.Range(fila, 1, fila, 5).Style.Font.FontColor = XLColor.FromHtml("#a9bcc6");
            fila++;
        }
        TotalesExcel(ws, fila, 3, new[] { 4, 5 }, montoCol: 5);
    }

    /// <summary>Título en la fila 1, encabezados en la 3. Devuelve la primera fila de datos.</summary>
    private static int EncabezadoExcel(IXLWorksheet ws, string titulo, params string[] columnas)
    {
        ws.Cell(1, 1).Value = titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 13;
        ws.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml(Azul);

        for (int i = 0; i < columnas.Length; i++)
        {
            var c = ws.Cell(3, i + 1);
            c.Value = columnas[i];
            c.Style.Font.Bold = true;
            c.Style.Fill.BackgroundColor = XLColor.FromHtml(Azul);
            c.Style.Font.FontColor = XLColor.White;
        }
        ws.SheetView.FreezeRows(3);
        return 4;
    }

    private static void TotalesExcel(IXLWorksheet ws, int fila, int colEtiqueta, int[] columnasSuma, int montoCol)
    {
        if (fila > 4)
        {
            ws.Range(3, 1, fila - 1, columnasSuma.Max()).SetAutoFilter();
            ws.Cell(fila, colEtiqueta).Value = "TOTAL";
            ws.Cell(fila, colEtiqueta).Style.Font.Bold = true;
            foreach (var col in columnasSuma)
            {
                var letra = XLHelper.GetColumnLetterFromNumber(col);
                ws.Cell(fila, col).FormulaA1 = $"SUM({letra}4:{letra}{fila - 1})";
                ws.Cell(fila, col).Style.Font.Bold = true;
            }
            ws.Range(fila, 1, fila, columnasSuma.Max()).Style.Border.TopBorder = XLBorderStyleValues.Thin;
        }
        ws.Column(montoCol).Style.NumberFormat.Format = "$#,##0.00";
        ws.Columns().AdjustToContents();
        ws.Column(1).Width = 6;
    }
}
