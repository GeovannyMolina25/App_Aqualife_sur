namespace rotter.Dominio.Interfaces.Servicios;

public record ResultadoArchivo(bool Exito, string? Url, string? Error);

public interface IArchivoServicio
{
    Task<ResultadoArchivo> GuardarComprobanteAsync(Stream contenido, string nombreOriginal, string contentType, long tamanoBytes);
    Task<ResultadoArchivo> GuardarImagenServicioAsync(Stream contenido, string nombreOriginal, string contentType, long tamanoBytes);
}
