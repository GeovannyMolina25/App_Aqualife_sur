using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Productos.Commands;

/// <summary>
/// Elimina un producto/servicio. Si ya tiene ventas o cotizaciones no se borra (se perdería el historial
/// de facturas y reportes): se oculta del catálogo y se informa. Devuelve true si se borró de verdad.
/// </summary>
public record EliminarProductoCommand(int ProductoId) : IRequest<RespuestaDto<bool>>;

public class EliminarProductoHandler : IRequestHandler<EliminarProductoCommand, RespuestaDto<bool>>
{
    private readonly IProductoRepositorio _productos;
    private readonly IAuditoriaServicio   _auditoria;

    public EliminarProductoHandler(IProductoRepositorio productos, IAuditoriaServicio auditoria)
    { _productos = productos; _auditoria = auditoria; }

    public async Task<RespuestaDto<bool>> Handle(EliminarProductoCommand req, CancellationToken ct)
    {
        var producto = await _productos.ObtenerPorIdAsync(req.ProductoId);
        if (producto is null) return RespuestaDto<bool>.Fallo("No se encontró el producto o servicio.", 404);

        if (await _productos.TieneHistorialAsync(producto.Id))
        {
            producto.Activo = false;
            await _productos.ActualizarAsync(producto);
            await _auditoria.RegistrarAsync("DESACTIVAR_PRODUCTO", "Productos", producto.Id.ToString(),
                datosNuevos: new { producto.Nombre, Motivo = "Eliminación con historial" });
            return RespuestaDto<bool>.Ok(false,
                $"\"{producto.Nombre}\" tiene ventas o cotizaciones registradas, así que se ocultó en lugar de eliminarse para conservar el historial.");
        }

        var nombre = producto.Nombre;
        await _productos.EliminarAsync(producto);
        await _auditoria.RegistrarAsync("ELIMINAR_PRODUCTO", "Productos", req.ProductoId.ToString(),
            datosAnteriores: new { Nombre = nombre });
        return RespuestaDto<bool>.Ok(true, $"\"{nombre}\" se eliminó.");
    }
}
