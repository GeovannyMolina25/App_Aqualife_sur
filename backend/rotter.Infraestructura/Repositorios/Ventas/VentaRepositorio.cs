using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Ventas;

public class VentaRepositorio : IVentaRepositorio
{
    private readonly RotterDbContext _db;
    public VentaRepositorio(RotterDbContext db) => _db = db;

    private IQueryable<Venta> ConIncludes() =>
        _db.Ventas.Include(v => v.Cliente).Include(v => v.Colaborador)
            .Include(v => v.Detalles).ThenInclude(d => d.Producto);

    public async Task<Venta?> ObtenerPorIdAsync(int id) =>
        await ConIncludes().FirstOrDefaultAsync(v => v.Id == id);

    public async Task<List<Venta>> ObtenerTodasAsync(int pagina, int tamano, DateTime? desde = null, DateTime? hasta = null)
    {
        var q = ConIncludes().AsQueryable();
        if (desde.HasValue) q = q.Where(v => v.FechaVenta >= desde.Value);
        if (hasta.HasValue) q = q.Where(v => v.FechaVenta <= hasta.Value);
        return await q.OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    }

    public async Task<List<Venta>> ObtenerPorClienteAsync(int clienteId, int pagina, int tamano) =>
        await ConIncludes().Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

    public async Task<List<Venta>> ObtenerPorColaboradorAsync(int colaboradorId, int pagina, int tamano) =>
        await ConIncludes().Where(v => v.ColaboradorId == colaboradorId)
            .OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

    public async Task<List<Venta>> ObtenerPorMesAsync(int año, int mes) =>
        await ConIncludes().Where(v => v.FechaVenta.Year == año && v.FechaVenta.Month == mes)
            .OrderByDescending(v => v.FechaVenta).ToListAsync();

    public async Task<List<Venta>> ObtenerPorColaboradorYPeriodoAsync(int colaboradorId, DateTime desde, DateTime hasta) =>
        await ConIncludes().Where(v => v.ColaboradorId == colaboradorId && v.FechaVenta >= desde && v.FechaVenta <= hasta)
            .OrderByDescending(v => v.FechaVenta).ToListAsync();

    public async Task<Venta> CrearAsync(Venta venta)
    { _db.Ventas.Add(venta); await _db.SaveChangesAsync(); return venta; }

    public async Task<string> GenerarNumeroVentaAsync()
    {
        var año = DateTime.UtcNow.Year;
        var n = await _db.Ventas.CountAsync(v => v.FechaVenta.Year == año) + 1;
        return $"VTA-{año}-{n:D4}";
    }

    public async Task<MetricasVentaData> ObtenerMetricasAsync()
    {
        var hoy = DateTime.UtcNow.Date;
        var mes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return new MetricasVentaData(
            await _db.Ventas.Where(v => v.FechaVenta.Date == hoy).SumAsync(v => (decimal?)v.Total) ?? 0,
            await _db.Ventas.Where(v => v.FechaVenta >= mes).SumAsync(v => (decimal?)v.Total) ?? 0,
            await _db.Ventas.CountAsync(v => v.FechaVenta.Date == hoy),
            await _db.Productos.CountAsync(p => p.Activo));
    }

    public async Task<List<TopColaboradorData>> ObtenerTopColaboradoresAsync(DateTime desde, DateTime hasta) =>
        await _db.Ventas.Where(v => v.FechaVenta >= desde && v.FechaVenta <= hasta)
            .GroupBy(v => new { v.ColaboradorId, v.Colaborador.Nombre, v.Colaborador.Apellido })
            .Select(g => new TopColaboradorData(g.Key.Nombre + " " + g.Key.Apellido, g.Count(), g.Sum(v => v.Total)))
            .OrderByDescending(t => t.MontoTotal).Take(10).ToListAsync();
}
