using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Productos;

public class ProductoRepositorio : IProductoRepositorio
{
    private readonly RotterDbContext _db;
    public ProductoRepositorio(RotterDbContext db) => _db = db;

    public async Task<Producto?> ObtenerPorIdAsync(int id) =>
        await _db.Productos.Include(p => p.Categoria).Include(p => p.CreadoPor)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<List<Producto>> ObtenerTodosAsync(int pagina, int tamano, bool soloActivos = true)
    {
        var q = _db.Productos.Include(p => p.Categoria).AsQueryable();
        if (soloActivos) q = q.Where(p => p.Activo);
        return await q.OrderBy(p => p.Nombre).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    }

    public async Task<List<Producto>> ObtenerPromocionesAsync() =>
        await _db.Productos.Include(p => p.Categoria)
            .Where(p => p.EsPromocion && p.Activo && (!p.FechaFinPromocion.HasValue || p.FechaFinPromocion > DateTime.Now))
            .ToListAsync();

    public async Task<int> TotalAsync(bool soloActivos = true)
    {
        var q = _db.Productos.AsQueryable();
        if (soloActivos) q = q.Where(p => p.Activo);
        return await q.CountAsync();
    }

    public async Task<Producto> CrearAsync(Producto producto)
    { _db.Productos.Add(producto); await _db.SaveChangesAsync(); return producto; }

    public async Task<Producto> ActualizarAsync(Producto producto)
    { producto.FechaModificacion = DateTime.Now; _db.Productos.Update(producto); await _db.SaveChangesAsync(); return producto; }

    public async Task ActualizarStockAsync(int productoId, int cantidad)
    {
        var p = await _db.Productos.FindAsync(productoId);
        if (p is null) return;
        p.Stock -= cantidad;
        await _db.SaveChangesAsync();
    }
}
