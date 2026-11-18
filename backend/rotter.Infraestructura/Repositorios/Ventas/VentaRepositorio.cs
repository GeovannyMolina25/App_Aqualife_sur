using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;
using rotter.Dominio.DTOs.Ventas;
using rotter.Dominio.Constantes;

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

    public async Task<List<Venta>> ObtenerTodasAsync(int pagina, int tamano, DateTime? desde = null, DateTime? hasta = null, string? busqueda = null)
    {
        var q = FiltrarVentas(ConIncludes(), desde, hasta, busqueda);
        return await q.OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
    }

    public async Task<int> TotalAsync(DateTime? desde = null, DateTime? hasta = null, string? busqueda = null) =>
        await FiltrarVentas(_db.Ventas.Include(v => v.Cliente), desde, hasta, busqueda).CountAsync();

    private static IQueryable<Venta> FiltrarVentas(IQueryable<Venta> query, DateTime? desde, DateTime? hasta, string? busqueda)
    {
        if (desde.HasValue) query = query.Where(v => v.FechaVenta >= desde.Value);
        if (hasta.HasValue) query = query.Where(v => v.FechaVenta <= hasta.Value);
        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var texto = busqueda.Trim();
            query = query.Where(v =>
                EF.Functions.Like(v.NumeroVenta, $"%{texto}%") ||
                EF.Functions.Like(v.Cliente.Nombre, $"%{texto}%") ||
                EF.Functions.Like(v.Cliente.Apellido, $"%{texto}%") ||
                EF.Functions.Like(v.Cliente.Email, $"%{texto}%"));
        }
        return query;
    }

    public async Task<List<Venta>> ObtenerPorClienteAsync(int clienteId, int pagina, int tamano) =>
        await ConIncludes().Where(v => v.ClienteId == clienteId)
            .OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

    public async Task<List<Venta>> ObtenerPorColaboradorAsync(int colaboradorId, int pagina, int tamano) =>
        await ConIncludes().Where(v => v.ColaboradorId == colaboradorId)
            .OrderByDescending(v => v.FechaVenta).Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

    public async Task<List<Venta>> ObtenerPorMesAsync(int anio, int mes) =>
    await ConIncludes()
        .Where(v =>
            v.FechaVenta.Year == anio &&
            v.FechaVenta.Month == mes)
        .OrderByDescending(v => v.FechaVenta)
        .ToListAsync();

    public async Task<List<Venta>> ObtenerPorColaboradorYPeriodoAsync(int colaboradorId, DateTime desde, DateTime hasta) =>
        await ConIncludes().Where(v => v.ColaboradorId == colaboradorId && v.FechaVenta >= desde && v.FechaVenta <= hasta)
            .OrderByDescending(v => v.FechaVenta).ToListAsync();

    public async Task<Venta> CrearAsync(Venta venta)
    { _db.Ventas.Add(venta); await _db.SaveChangesAsync(); return venta; }

    public async Task<bool> ActualizarEstadoAsync(int ventaId, string nuevoEstado)
    {
        var venta = await _db.Ventas.FindAsync(ventaId);
        if (venta is null) return false;
        venta.Estado = nuevoEstado;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarComprobanteAsync(int ventaId, string comprobanteUrl, string estadoPago)
    {
        var venta = await _db.Ventas.FindAsync(ventaId);
        if (venta is null) return false;
        venta.ComprobanteUrl = comprobanteUrl;
        venta.EstadoPago = estadoPago;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarEstadoPagoAsync(int ventaId, string estadoPago)
    {
        var venta = await _db.Ventas.FindAsync(ventaId);
        if (venta is null) return false;
        venta.EstadoPago = estadoPago;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerarNumeroVentaAsync()
    {
        var Anio = DateTime.Now.Year;
        var n = await _db.Ventas.CountAsync(v => v.FechaVenta.Year == Anio) + 1;
        return $"VTA-{Anio}-{n:D4}";
    }

    public async Task<MetricasVentaData> ObtenerMetricasAsync()
    {
        var hoy = DateTime.Now.Date;
        var mes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        return new MetricasVentaData(
            await _db.Ventas.Where(v => v.FechaVenta.Date == hoy).SumAsync(v => (decimal?)v.Total) ?? 0,
            await _db.Ventas.Where(v => v.FechaVenta >= mes).SumAsync(v => (decimal?)v.Total) ?? 0,
            await _db.Ventas.CountAsync(v => v.FechaVenta.Date == hoy),
            await _db.Productos.CountAsync(p => p.Activo));
    }

    public async Task<List<TopColaboradorData>>ObtenerTopColaboradoresAsync(DateTime desde, DateTime hasta)
    {
        var ventas = await _db.Ventas
            .Include(v => v.Colaborador)
            .Where(v =>
                v.FechaVenta >= desde &&
                v.FechaVenta <= hasta)
            .ToListAsync();

        return ventas
            .GroupBy(v => new
            {
                v.ColaboradorId,
                Nombre = v.Colaborador?.Nombre ?? "",
                Apellido = v.Colaborador?.Apellido ?? ""
            })
            .Select(g => new TopColaboradorData(
                $"{g.Key.Nombre} {g.Key.Apellido}",
                g.Count(),
                g.Sum(v => v.Total)
            ))
            .OrderByDescending(x => x.MontoTotal)
            .Take(10)
            .ToList();
    }
    /// <summary>Ventas no anuladas dentro del rango (ambos extremos opcionales; <paramref name="hasta"/> incluye todo ese día).</summary>
    private IQueryable<Venta> VentasValidas(DateTime? desde, DateTime? hasta)
    {
        var q = _db.Ventas.Where(v => v.Estado != "Anulada");
        if (desde.HasValue) q = q.Where(v => v.FechaVenta >= desde.Value.Date);
        if (hasta.HasValue) q = q.Where(v => v.FechaVenta < hasta.Value.Date.AddDays(1));
        return q;
    }

    /// <summary>
    /// Productos (o servicios) ordenados por unidades vendidas, sin ventas anuladas.
    /// Un ítem es servicio si su categoría se llama "Servicio…" (p. ej. "Servicios de Limpieza").
    /// <paramref name="cantidad"/> null = todos; <paramref name="incluirSinVentas"/> agrega al final los activos que no se vendieron.
    /// </summary>
    public async Task<List<TopProductoData>> ObtenerTopProductosAsync(int? cantidad, bool servicios = false,
        DateTime? desde = null, DateTime? hasta = null, bool incluirSinVentas = false)
    {
        var ventas = VentasValidas(desde, hasta);
        var consulta = _db.DetalleVentas
            .Where(d => ventas.Any(v => v.Id == d.VentaId)
                && d.Producto.Categoria.Nombre.Contains("Servicio") == servicios)
            .GroupBy(d => new { d.ProductoId, d.Producto.Nombre, Categoria = d.Producto.Categoria.Nombre })
            .Select(g => new { g.Key.ProductoId, g.Key.Nombre, g.Key.Categoria, Unidades = g.Sum(d => d.Cantidad), Total = g.Sum(d => d.Subtotal) })
            .OrderByDescending(x => x.Unidades)
            .ThenByDescending(x => x.Total);

        var top = cantidad.HasValue
            ? await consulta.Take(cantidad.Value).ToListAsync()
            : await consulta.ToListAsync();

        var resultado = top.Select(t => new TopProductoData(t.Nombre, t.Unidades, t.Total, t.Categoria)).ToList();
        if (!incluirSinVentas) return resultado;

        var vendidos = top.Select(t => t.ProductoId).ToList();
        var sinVentas = await _db.Productos
            .Where(p => p.Activo && !vendidos.Contains(p.Id)
                && p.Categoria.Nombre.Contains("Servicio") == servicios)
            .OrderBy(p => p.Nombre)
            .Select(p => new TopProductoData(p.Nombre, 0, 0m, p.Categoria.Nombre))
            .ToListAsync();

        resultado.AddRange(sinVentas);
        return resultado;
    }

    /// <summary>
    /// Clientes ordenados por total gastado, sin ventas anuladas. Se excluyen el cliente genérico
    /// de mostrador y el usuario sistema de pedidos web, que no son compradores reales.
    /// <paramref name="cantidad"/> null = todos.
    /// </summary>
    public async Task<List<TopClienteData>> ObtenerTopClientesAsync(int? cantidad, DateTime? desde = null, DateTime? hasta = null)
    {
        var ventasValidas = VentasValidas(desde, hasta).Where(v =>
            v.ClienteId != EmpresaConstantes.ClienteGenericoId
            && v.Cliente.Email != EmpresaConstantes.EmailUsuarioSistemaWeb);

        var consulta = ventasValidas
            .GroupBy(v => new { v.ClienteId, v.Cliente.Nombre, v.Cliente.Apellido, v.Cliente.Email })
            .Select(g => new
            {
                g.Key.ClienteId, g.Key.Nombre, g.Key.Apellido, g.Key.Email,
                Compras = g.Count(), Total = g.Sum(v => v.Total), Ultima = g.Max(v => v.FechaVenta)
            })
            .OrderByDescending(x => x.Total);

        var top = cantidad.HasValue
            ? await consulta.Take(cantidad.Value).ToListAsync()
            : await consulta.ToListAsync();

        var ids = top.Select(t => t.ClienteId).ToList();
        var unidades = await _db.DetalleVentas
            .Where(d => ids.Contains(d.Venta.ClienteId) && ventasValidas.Any(v => v.Id == d.VentaId))
            .GroupBy(d => d.Venta.ClienteId)
            .Select(g => new { ClienteId = g.Key, Unidades = g.Sum(d => d.Cantidad) })
            .ToDictionaryAsync(x => x.ClienteId, x => x.Unidades);

        return top.Select(t => new TopClienteData(
                $"{t.Nombre} {t.Apellido}", t.Email, t.Compras,
                unidades.GetValueOrDefault(t.ClienteId), t.Total, t.Ultima))
            .ToList();
    }

    public async Task<FacturaDto?> ObtenerFacturaPorIdAsync(int ventaId)
    {
        var venta = await ConIncludes()
            .FirstOrDefaultAsync(v => v.Id == ventaId);

        if (venta == null)
            return null;

        // Ventas creadas antes de existir estas columnas quedan con Subtotal/Impuestos en 0;
        // para esas se conserva el comportamiento histórico (factura sin IVA desglosado).
        var subtotal = venta.Subtotal > 0 ? venta.Subtotal : venta.Total;
        var iva = venta.Subtotal > 0 ? venta.Impuestos : 0;

        return new FacturaDto
        {
            NumeroFactura = venta.NumeroVenta,

            Fecha = venta.FechaVenta,

            Cliente =
                $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}",

            CedulaCliente = "",

            EmailCliente = venta.Cliente.Email,

            DireccionCliente =
                venta.Cliente.Direccion,

            Subtotal = subtotal,

            Iva = iva,

            Total = venta.Total,

            Detalles = venta.Detalles.Select(d =>
                new FacturaDetalleDto(
                    d.Producto.Nombre,
                    d.Producto.Caracteristicas,
                    d.Cantidad,
                    d.PrecioUnitario,
                    d.Subtotal
                )
            ).ToList()
        };
    }
}
