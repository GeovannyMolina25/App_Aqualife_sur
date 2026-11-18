using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Productos.Commands;

/// <summary>Activa o desactiva un producto/servicio. Desactivado deja de verse en el catálogo, pero conserva su historial de ventas.</summary>
public record CambiarEstadoProductoCommand(int ProductoId, bool Activo) : IRequest<RespuestaDto<bool>>;

public class CambiarEstadoProductoHandler : IRequestHandler<CambiarEstadoProductoCommand, RespuestaDto<bool>>
{
    private readonly IProductoRepositorio _productos;
    private readonly IAuditoriaServicio   _auditoria;

    public CambiarEstadoProductoHandler(IProductoRepositorio productos, IAuditoriaServicio auditoria)
    { _productos = productos; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(CambiarEstadoProductoCommand req, CancellationToken ct)
    {
        var producto = await _productos.ObtenerPorIdAsync(req.ProductoId);
        if (producto is null) return RespuestaDto<bool>.Fallo("No se encontró el producto o servicio.", 404);

        producto.Activo = req.Activo;
        await _productos.ActualizarAsync(producto);
        await _auditoria.RegistrarAsync(req.Activo ? "ACTIVAR_PRODUCTO" : "DESACTIVAR_PRODUCTO", "Productos", producto.Id.ToString(),
            datosNuevos: new { producto.Nombre, producto.Activo });

        return RespuestaDto<bool>.Ok(true, req.Activo ? "Publicado de nuevo." : "Ocultado del catálogo.");
    }
}
