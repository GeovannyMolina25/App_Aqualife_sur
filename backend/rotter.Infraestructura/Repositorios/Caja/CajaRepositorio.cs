using Microsoft.EntityFrameworkCore;
using rotter.Dominio.DTOs.Caja;
using rotter.Dominio.DTOs.Comun;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Caja;

public class CajaRepositorio : ICajaRepositorio
{
    private readonly RotterDbContext _db;
    public CajaRepositorio(RotterDbContext db) => _db = db;

    public async Task<string> GenerarNumeroIngresoAsync()
    {
        var anio = DateTime.Now.Year;
        var n = await _db.IngresoCaja.CountAsync(i => i.FechaIngreso.Year == anio) + 1;
        return $"ING-{anio}-{n:D4}";
    }

    public async Task<string> GenerarNumeroSalidaAsync()
    {
        var anio = DateTime.Now.Year;
        var n = await _db.SalidaCaja.CountAsync(s => s.FechaSalida.Year == anio) + 1;
        return $"SAL-{anio}-{n:D4}";
    }

    public async Task<IngresoCaja> CrearIngresoAsync(IngresoCaja ingreso, List<int> ventaIds)
    {
        _db.IngresoCaja.Add(ingreso);
        await _db.SaveChangesAsync();

        if (ventaIds.Count > 0)
        {
            foreach (var ventaId in ventaIds)
                _db.DetalleIngresoCaja.Add(new DetalleIngresoCaja { IngresoCajaId = ingreso.Id, VentaId = ventaId });

            var ventas = await _db.Ventas.Where(v => ventaIds.Contains(v.Id)).ToListAsync();
            foreach (var venta in ventas) venta.Estado = "Ingresada";

            await _db.SaveChangesAsync();
        }

        return ingreso;
    }

    public async Task<SalidaCaja> CrearSalidaAsync(SalidaCaja salida)
    {
        _db.SalidaCaja.Add(salida);
        await _db.SaveChangesAsync();
        return salida;
    }

    public async Task<IngresoCaja?> ObtenerIngresoPorIdAsync(int id) =>
        await _db.IngresoCaja.Include(i => i.Usuario).Include(i => i.Detalles)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<SaldoCajaDto> ObtenerSaldoAsync()
    {
        var totalIngresos = await _db.IngresoCaja.Where(i => i.Activo).SumAsync(i => (decimal?)i.TotalIngresado) ?? 0;
        var totalSalidas = await _db.SalidaCaja.Where(s => s.Activo).SumAsync(s => (decimal?)s.Valor) ?? 0;
        return new SaldoCajaDto(totalIngresos, totalSalidas, totalIngresos - totalSalidas);
    }

    public async Task<PagedResult<MovimientoCajaDto>> ObtenerHistorialAsync(int pagina, int tamano, string? busqueda = null)
    {
        var ingresos = await _db.IngresoCaja.Include(i => i.Usuario).Where(i => i.Activo)
            .Select(i => new MovimientoCajaDto(i.Id, "Ingreso", i.NumeroIngreso, i.FechaIngreso, i.TotalIngresado,
                i.Usuario.Nombre + " " + i.Usuario.Apellido, i.Banco + " - " + i.NumeroTransaccion))
            .ToListAsync();

        var salidas = await _db.SalidaCaja.Include(s => s.Usuario).Where(s => s.Activo)
            .Select(s => new MovimientoCajaDto(s.Id, "Salida", s.NumeroSalida, s.FechaSalida, s.Valor,
                s.Usuario.Nombre + " " + s.Usuario.Apellido, s.Motivo))
            .ToListAsync();

        var movimientos = ingresos.Concat(salidas).OrderByDescending(m => m.Fecha).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            movimientos = movimientos.Where(m =>
                m.Numero.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                m.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                m.NombreUsuario.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                (m.Detalle != null && m.Detalle.Contains(texto, StringComparison.OrdinalIgnoreCase)));
        }

        var lista = movimientos.ToList();
        var total = lista.Count;
        var items = lista.Skip((pagina - 1) * tamano).Take(tamano).ToList();
        var totalPaginas = (int)Math.Ceiling(total / (double)tamano);

        return new PagedResult<MovimientoCajaDto>(items, total, pagina, tamano, totalPaginas);
    }

    public async Task<List<Venta>> ObtenerVentasPendientesDeIngresoAsync() =>
        await _db.Ventas.Include(v => v.Cliente).Include(v => v.Colaborador).Include(v => v.Detalles).ThenInclude(d => d.Producto)
            .Where(v => v.Estado == "Pendiente" || v.Estado == "Completada")
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
}
