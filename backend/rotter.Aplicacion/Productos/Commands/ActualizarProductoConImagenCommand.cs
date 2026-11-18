using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Productos.Commands;

/// <summary>
/// Edita un producto o servicio. La imagen es opcional: si no se envía, se conserva la actual.
/// El stock no se toca aquí (los servicios no manejan inventario).
/// </summary>
public record ActualizarProductoConImagenCommand(
    int ProductoId,
    string Nombre,
    string? Descripcion,
    string? Caracteristicas,
    int CategoriaId,
    decimal Precio,
    bool EsPromocion,
    decimal? PrecioPromocion,
    int UsuarioId,
    Stream? ImagenContenido,
    string? ImagenNombreOriginal,
    string? ImagenContentType,
    long ImagenTamanoBytes
) : IRequest<RespuestaDto<ProductoDto>>;

public class ActualizarProductoConImagenHandler : IRequestHandler<ActualizarProductoConImagenCommand, RespuestaDto<ProductoDto>>
{
    private readonly IProductoRepositorio _productos;
    private readonly IArchivoServicio     _archivos;
    private readonly IAuditoriaServicio   _auditoria;

    public ActualizarProductoConImagenHandler(IProductoRepositorio productos, IArchivoServicio archivos, IAuditoriaServicio auditoria)
    { _productos = productos; _archivos = archivos; _auditoria = auditoria; }

    public async Task<RespuestaDto<ProductoDto>> Handle(ActualizarProductoConImagenCommand req, CancellationToken ct)
    {
        var producto = await _productos.ObtenerPorIdAsync(req.ProductoId);
        if (producto is null) return RespuestaDto<ProductoDto>.Fallo("No se encontró el producto o servicio.", 404);
        if (string.IsNullOrWhiteSpace(req.Nombre)) return RespuestaDto<ProductoDto>.Fallo("El nombre es obligatorio.");
        if (req.CategoriaId <= 0) return RespuestaDto<ProductoDto>.Fallo("Selecciona una categoría.");
        var errorPrecio = ValidacionPrecios.Validar(req.Precio, req.EsPromocion, req.PrecioPromocion);
        if (errorPrecio is not null) return RespuestaDto<ProductoDto>.Fallo(errorPrecio);

        if (req.ImagenContenido is not null && req.ImagenTamanoBytes > 0)
        {
            var resultado = await _archivos.GuardarImagenServicioAsync(
                req.ImagenContenido, req.ImagenNombreOriginal ?? "imagen", req.ImagenContentType ?? "", req.ImagenTamanoBytes);
            if (!resultado.Exito) return RespuestaDto<ProductoDto>.Fallo(resultado.Error!, 400);
            producto.ImagenUrl = resultado.Url;
        }

        var anterior = new { producto.Nombre, producto.Precio, producto.EsPromocion, producto.PrecioPromocion };

        producto.Nombre          = req.Nombre.Trim();
        producto.Descripcion     = req.Descripcion?.Trim();
        producto.Caracteristicas = req.Caracteristicas?.Trim();
        producto.CategoriaId     = req.CategoriaId;
        producto.Precio          = req.Precio;
        producto.EsPromocion     = req.EsPromocion;
        producto.PrecioPromocion = req.EsPromocion ? req.PrecioPromocion : null;

        await _productos.ActualizarAsync(producto);
        await _auditoria.RegistrarAsync("ACTUALIZAR_PRODUCTO", "Productos", producto.Id.ToString(),
            datosAnteriores: anterior,
            datosNuevos: new { producto.Nombre, producto.Precio, producto.EsPromocion, producto.PrecioPromocion });

        var actualizado = await _productos.ObtenerPorIdAsync(producto.Id);
        return RespuestaDto<ProductoDto>.Ok(CrearProductoHandler.Mapear(actualizado!), "Cambios guardados.");
    }
}
