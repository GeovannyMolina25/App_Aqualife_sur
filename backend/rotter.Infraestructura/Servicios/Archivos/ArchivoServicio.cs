using Microsoft.AspNetCore.Hosting;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Archivos;

public class ArchivoServicio : IArchivoServicio
{
    private static readonly Dictionary<string, string> ExtensionesPermitidas = new()
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".pdf"] = "application/pdf",
    };

    private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5MB

    private readonly IWebHostEnvironment _env;
    public ArchivoServicio(IWebHostEnvironment env) => _env = env;

    public async Task<ResultadoArchivo> GuardarComprobanteAsync(Stream contenido, string nombreOriginal, string contentType, long tamanoBytes)
    {
        if (tamanoBytes <= 0)
            return new ResultadoArchivo(false, null, "El archivo está vacío.");

        if (tamanoBytes > TamanoMaximoBytes)
            return new ResultadoArchivo(false, null, "El archivo supera el tamaño máximo permitido (5MB).");

        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!ExtensionesPermitidas.TryGetValue(extension, out var contentTypeEsperado))
            return new ResultadoArchivo(false, null, "Formato no permitido. Sube una imagen (JPG/PNG) o un PDF.");

        if (!string.Equals(contentType, contentTypeEsperado, StringComparison.OrdinalIgnoreCase))
            return new ResultadoArchivo(false, null, "El contenido del archivo no coincide con su extensión.");

        var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var carpeta = Path.Combine(webRoot, "uploads", "comprobantes");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        using (var destino = new FileStream(rutaCompleta, FileMode.Create))
            await contenido.CopyToAsync(destino);

        return new ResultadoArchivo(true, $"/uploads/comprobantes/{nombreArchivo}", null);
    }

    private static readonly Dictionary<string, string> ExtensionesImagenPermitidas = new()
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
    };

    public async Task<ResultadoArchivo> GuardarImagenServicioAsync(Stream contenido, string nombreOriginal, string contentType, long tamanoBytes)
    {
        if (tamanoBytes <= 0)
            return new ResultadoArchivo(false, null, "La imagen está vacía.");

        if (tamanoBytes > TamanoMaximoBytes)
            return new ResultadoArchivo(false, null, "La imagen supera el tamaño máximo permitido (5MB).");

        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!ExtensionesImagenPermitidas.TryGetValue(extension, out var contentTypeEsperado))
            return new ResultadoArchivo(false, null, "Formato no permitido. Sube una imagen JPG o PNG.");

        if (!string.Equals(contentType, contentTypeEsperado, StringComparison.OrdinalIgnoreCase))
            return new ResultadoArchivo(false, null, "El contenido del archivo no coincide con su extensión.");

        var webRoot = string.IsNullOrEmpty(_env.WebRootPath)
            ? Path.Combine(_env.ContentRootPath, "wwwroot")
            : _env.WebRootPath;

        var carpeta = Path.Combine(webRoot, "uploads", "servicios");
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

        using (var destino = new FileStream(rutaCompleta, FileMode.Create))
            await contenido.CopyToAsync(destino);

        return new ResultadoArchivo(true, $"/uploads/servicios/{nombreArchivo}", null);
    }
}
