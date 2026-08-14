using MediatR;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.DTOs.Productos;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Aplicacion.Productos.Commands;

public record CrearProductoCommand(CrearProductoDto Dto, int UsuarioId) : IRequest<RespuestaDto<ProductoDto>>;

public class CrearProductoHandler : IRequestHandler<CrearProductoCommand, RespuestaDto<ProductoDto>>
{
    private readonly IProductoRepositorio _productos;
    private readonly IAuditoriaServicio   _auditoria;

    public CrearProductoHandler(IProductoRepositorio productos, IAuditoriaServicio auditoria)
    { _productos = productos; _auditoria = auditoria; }

    public async Task<RespuestaDto<ProductoDto>> Handle(CrearProductoCommand req, CancellationToken ct)
    {
        var producto = new Producto
        {
            Nombre = req.Dto.Nombre, Descripcion = req.Dto.Descripcion, Caracteristicas = req.Dto.Caracteristicas,
            Precio = req.Dto.Precio, Stock = req.Dto.Stock, CategoriaId = req.Dto.CategoriaId,
            ImagenUrl = req.Dto.ImagenUrl, EsPromocion = req.Dto.EsPromocion,
            PrecioPromocion = req.Dto.PrecioPromocion, FechaInicioPromocion = req.Dto.FechaInicioPromocion,
            FechaFinPromocion = req.Dto.FechaFinPromocion, CreadoPorId = req.UsuarioId
        };

        await _productos.CrearAsync(producto);
        await _auditoria.RegistrarAsync("CREAR_PRODUCTO", "Productos", producto.Id.ToString(),
            datosNuevos: new { producto.Nombre, producto.Precio });

        var creado = await _productos.ObtenerPorIdAsync(producto.Id);
        return RespuestaDto<ProductoDto>.Ok(Mapear(creado!), "Producto creado exitosamente.");
    }

    public static ProductoDto Mapear(Producto p) => new(p.Id, p.Nombre, p.Descripcion, p.Caracteristicas,
        p.Precio, p.Stock, p.CategoriaId, p.Categoria.Nombre, p.Categoria.Tipo, p.ImagenUrl, p.EsPromocion, p.PrecioPromocion,
        p.FechaInicioPromocion, p.FechaFinPromocion, p.Activo);
}
