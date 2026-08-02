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

    public async Task<List<Producto>> ObtenerTodosAsync(int pagina, int tamano, bool soloActivos = true, string? busqueda = null)
    {
        var q = _db.Productos.Include(p => p.Categoria).AsQueryable();
        if (soloActivos) q = q.Where(p => p.Activo);
        q = FiltrarPorBusqueda(q, busqueda);
        return await q.OrderBy(p => p.Nombre).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    }

    public async Task<List<Producto>> ObtenerPromocionesAsync() =>
        await _db.Productos.Include(p => p.Categoria)
            .Where(p => p.EsPromocion && p.Activo && (!p.FechaFinPromocion.HasValue || p.FechaFinPromocion > DateTime.Now))
            .ToListAsync();

    public async Task<int> TotalAsync(bool soloActivos = true, string? busqueda = null)
    {
        var q = _db.Productos.AsQueryable();
        if (soloActivos) q = q.Where(p => p.Activo);
        q = FiltrarPorBusqueda(q, busqueda);
        return await q.CountAsync();
    }

    private static IQueryable<Producto> FiltrarPorBusqueda(IQueryable<Producto> query, string? busqueda)
    {
        if (string.IsNullOrWhiteSpace(busqueda)) return query;
        var texto = busqueda.Trim();
        return query.Where(p =>
            EF.Functions.Like(p.Nombre, $"%{texto}%") ||
            (p.Descripcion != null && EF.Functions.Like(p.Descripcion, $"%{texto}%")));
    }

    public async Task<Producto> CrearAsync(Producto producto)
    { _db.Productos.Add(producto); await _db.SaveChangesAsync(); return producto; }

    public async Task<Producto> ActualizarAsync(Producto producto)
    { producto.FechaModificacion = DateTime.Now; _db.Productos.Update(producto); await _db.SaveChangesAsync(); return producto; }

    public async Task<bool> ActualizarStockAsync(int productoId, int cantidad)
    {
        var filas = await _db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Productos SET Stock = Stock - {cantidad} WHERE Id = {productoId} AND Stock >= {cantidad}");
        return filas > 0;
    }
}
