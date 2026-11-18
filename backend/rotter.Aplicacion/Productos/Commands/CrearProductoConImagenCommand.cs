using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Productos.Commands;

/// <summary>
/// Crea un producto (o un servicio, según el Tipo de su categoría) subiendo la imagen como archivo
/// en vez de pegar una URL — usado desde la pantalla de catálogo cuando el personal agrega un nuevo
/// ítem con foto directamente. Delega la creación en sí a <see cref="CrearProductoCommand"/> para no
/// duplicar la lógica de validación/auditoría.
/// </summary>
public record CrearProductoConImagenCommand(
    string Nombre,
    string? Descripcion,
    int CategoriaId,
    decimal Precio,
    int Stock,
    int UsuarioId,
    Stream? ImagenContenido,
    string? ImagenNombreOriginal,
    string? ImagenContentType,
    long ImagenTamanoBytes
) : IRequest<RespuestaDto<ProductoDto>>;

public class CrearProductoConImagenHandler : IRequestHandler<CrearProductoConImagenCommand, RespuestaDto<ProductoDto>>
{
    private readonly IMediator _mediator;
    private readonly IArchivoServicio _archivos;

    public CrearProductoConImagenHandler(IMediator mediator, IArchivoServicio archivos)
    { _mediator = mediator; _archivos = archivos; }

    public async Task<RespuestaDto<ProductoDto>> Handle(CrearProductoConImagenCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre))
            return RespuestaDto<ProductoDto>.Fallo("El nombre es obligatorio.");
        if (req.CategoriaId <= 0)
            return RespuestaDto<ProductoDto>.Fallo("Selecciona una categoría.");

        string? imagenUrl = null;
        if (req.ImagenContenido is not null && req.ImagenTamanoBytes > 0)
        {
            var resultado = await _archivos.GuardarImagenServicioAsync(
                req.ImagenContenido, req.ImagenNombreOriginal ?? "imagen", req.ImagenContentType ?? "", req.ImagenTamanoBytes);
            if (!resultado.Exito) return RespuestaDto<ProductoDto>.Fallo(resultado.Error!, 400);
            imagenUrl = resultado.Url;
        }

        var dto = new CrearProductoDto(req.Nombre, req.Descripcion, null, req.Precio, req.Stock, req.CategoriaId,
            imagenUrl, false, null, null, null);

        return await _mediator.Send(new CrearProductoCommand(dto, req.UsuarioId), ct);
    }
}
