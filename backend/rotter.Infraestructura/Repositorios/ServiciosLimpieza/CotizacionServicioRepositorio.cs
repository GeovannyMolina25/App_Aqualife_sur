using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.ServiciosLimpieza;

public class CotizacionServicioRepositorio : ICotizacionServicioRepositorio
{
    private readonly RotterDbContext _db;
    public CotizacionServicioRepositorio(RotterDbContext db) => _db = db;

    public async Task<CotizacionServicio> CrearAsync(CotizacionServicio cotizacion)
    {
        _db.CotizacionesServicio.Add(cotizacion);
        await _db.SaveChangesAsync();
        return (await _db.CotizacionesServicio.Include(c => c.Producto)
            .FirstAsync(c => c.Id == cotizacion.Id));
    }

    public async Task<CotizacionServicio?> ObtenerPorIdAsync(int id) =>
        await _db.CotizacionesServicio.Include(c => c.Producto).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<CotizacionServicio>> ObtenerTodasAsync(int pagina, int tamano, string? estado = null, string? busqueda = null)
    {
        var q = Filtrar(estado, busqueda);
        return await q.OrderByDescending(c => c.FechaCreacion).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    }

    public async Task<int> TotalAsync(string? estado = null, string? busqueda = null) =>
        await Filtrar(estado, busqueda).CountAsync();

    private IQueryable<CotizacionServicio> Filtrar(string? estado, string? busqueda)
    {
        var q = _db.CotizacionesServicio.Include(c => c.Producto).AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(c => c.Estado == estado);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            q = q.Where(c =>
                EF.Functions.Like(c.NombreContacto, $"%{texto}%") ||
                EF.Functions.Like(c.Telefono, $"%{texto}%") ||
                EF.Functions.Like(c.Producto.Nombre, $"%{texto}%"));
        }
        return q;
    }

    public async Task<CotizacionServicio> ActualizarAsync(CotizacionServicio cotizacion)
    {
        cotizacion.FechaModificacion = DateTime.Now;
        _db.CotizacionesServicio.Update(cotizacion);
        await _db.SaveChangesAsync();
        return cotizacion;
    }
}
