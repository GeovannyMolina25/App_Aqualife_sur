using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using rotter.Dominio.Constantes;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Pdf;

public class FacturaPdfService : IFacturaPdfService
{
    public byte[] GenerarFactura(FacturaDto factura)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var documento = new FacturaPdfDocument(factura);

        return documento.GeneratePdf();
    }
}