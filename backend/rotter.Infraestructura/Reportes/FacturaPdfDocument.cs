using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using rotter.Dominio.Constantes;
using rotter.Dominio.DTOs.Ventas;

public class FacturaPdfDocument : IDocument
{
    private static readonly Color Azul = Colors.Blue.Darken2;
    private static readonly Color Celeste = Colors.Blue.Lighten4;
    private static readonly Color Gris = Colors.Grey.Lighten3;
    private static readonly Color Blanco = Colors.White;

    private readonly FacturaDto _factura;

    public FacturaPdfDocument(FacturaDto factura)
    {
        _factura = factura;
    }

    public DocumentMetadata GetMetadata()
        => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);

            page.Header()
                .Element(Cabecera);

            page.Content()
                .PaddingVertical(15)
                .Column(col =>
                {
                    col.Item().Element(DatosCliente);

                    col.Item().PaddingVertical(15);

                    col.Item().Element(TablaProductos);

                    col.Item().PaddingVertical(15);

                    col.Item().Element(Totales);

                    col.Item().PaddingVertical(25);

                    col.Item().Element(Firmas);
                });

            page.Footer()
                .Column(col =>
                {
                    col.Item().LineHorizontal(1);

                    col.Item()
                        .AlignCenter()
                        .Text("💧 El agua es la fuente de la vida. Mantente hidratado todos los días.")
                        .Italic()
                        .FontSize(9)
                        .FontColor("#1ABC9C");
                });
        });
    }
    void Cabecera(IContainer container)
    {
        container
            .Background(Azul)
            .Padding(15)
            .Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item()
                        .Text(EmpresaConstantes.Nombre)
                        .FontSize(26)
                        .Bold()
                        .FontColor(Blanco);

                    col.Item()
                        .Text("Purificadora y Distribuidora de Agua")
                        .FontSize(11)
                        .Italic()
                        .FontColor(Blanco);

                    col.Item().PaddingTop(5);

                    col.Item()
                        .Text($"RUC: {EmpresaConstantes.Ruc}")
                        .FontColor(Blanco)
                        .FontSize(9);

                    col.Item()
                        .Text($"Representante: {EmpresaConstantes.RepresentanteLegal}")
                        .FontSize(9)
                        .FontColor(Blanco);

                    col.Item()
                        .Text($"Teléfono: {EmpresaConstantes.Telefono}")
                        .FontColor(Blanco)
                        .FontSize(9); ;

                    col.Item()
                        .Text(EmpresaConstantes.Correo)
                        .FontColor(Blanco)
                        .FontSize(9);

                });

                row.ConstantItem(180)
                    .Background(Blanco)
                    .Padding(10)
                    .Column(col =>
                    {
                        col.Item().Text("FACTURA")
                            .Bold()
                            .FontSize(18);

                        col.Item().Text(_factura.NumeroFactura);

                        col.Item().Text(
                            _factura.Fecha.ToString("dd/MM/yyyy"));
                    });
            });
    }
    void DatosCliente(IContainer container)
    {
        container
            .Border(1)
            .BorderColor(Azul)
            .Padding(10)
            .Column(col =>
            {
                col.Item()
                    .Background(Celeste)
                    .Padding(5)
                    .Text("DATOS DEL CLIENTE")
                    .Bold();

                col.Item().Text($"Cliente: {_factura.Cliente}");

                col.Item().Text($"Cédula: {_factura.CedulaCliente}");

                col.Item().Text($"Dirección: {_factura.DireccionCliente}");

                col.Item().Text($"Correo: {_factura.EmailCliente}");
            });
    }
    private void TablaProductos(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.ConstantColumn(50);
                c.RelativeColumn(3);
                c.RelativeColumn(3);
                c.ConstantColumn(70);
                c.ConstantColumn(80);
            });
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Cant.").FontColor("#FFFFFF").Bold();
                header.Cell().Element(CellStyle).Text("Producto").FontColor("#FFFFFF").Bold();
                header.Cell().Element(CellStyle).Text("Características").FontColor("#FFFFFF").Bold();
                header.Cell().Element(CellStyle).Text("Precio").FontColor("#FFFFFF").Bold();
                header.Cell().Element(CellStyle).Text("Subtotal").FontColor("#FFFFFF").Bold();
            });

            IContainer CellStyle(IContainer c)
            {
                return c
                    .Background("#1a6b8a")
                    .Padding(6)
                    .Border(1)
                    .BorderColor("#FFFFFF");
            }
            int fila = 0;

            if (_factura.Detalles == null || !_factura.Detalles.Any())
            {
                table.Cell().ColumnSpan(5)
                    .Padding(10)
                    .Text("No existen productos en esta factura.");
                return;
            }
            foreach (var item in _factura.Detalles)
            {
                var color = fila % 2 == 0 ? "#FFFFFF" : "#F2F8FA";

                table.Cell().Background(color).Padding(5).Text(item.Cantidad.ToString());
                table.Cell().Background(color).Padding(5).Text(item.Producto);
                table.Cell().Background(color).Padding(5).Text(item.Caracteristicas);
                table.Cell().Background(color).Padding(5).AlignRight().Text($"${item.PrecioUnitario:N2}");
                table.Cell().Background(color).Padding(5).AlignRight().Text($"${item.Subtotal:N2}");

                fila++;
            }
        });
    }
    private void Totales(IContainer container)
    {
        var subtotal = _factura.Total / 1.15m;
        var iva = _factura.Total - subtotal;

        container.AlignRight()
            .Width(250)
            .Border(1)
            .BorderColor("#1ABC9C")
            .Padding(10)
            .Column(col =>
            {
                col.Item().Text($"Subtotal: ${subtotal:N2}");

                col.Item().Text($"IVA 15%: ${iva:N2}");

                col.Item()
                    .Text($"TOTAL: ${_factura.Total:N2}")
                    .Bold()
                    .FontSize(16)
                    .FontColor("#1ABC9C");
            });
    }

    private void Firmas(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem()
                .AlignCenter()
                .Column(col =>
                {
                    col.Item().PaddingTop(30);

                    col.Item().LineHorizontal(1);

                    col.Item().Text("Firma Empresa");
                });

            row.RelativeItem()
                .AlignCenter()
                .Column(col =>
                {
                    col.Item().PaddingTop(30);

                    col.Item().LineHorizontal(1);

                    col.Item().Text("Firma Cliente");
                });
        });
    }
}