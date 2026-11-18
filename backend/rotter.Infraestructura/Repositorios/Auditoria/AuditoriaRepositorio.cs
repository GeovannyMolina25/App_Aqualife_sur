using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Auditoria;

public class AuditoriaRepositorio : IAuditoriaRepositorio
{
    private readonly RotterDbContext _db;
    public AuditoriaRepositorio(RotterDbContext db) => _db = db;

    public async Task RegistrarAsync(rotter.Dominio.Entidades.Auditoria auditoria)
    { _db.Auditorias.Add(auditoria); await _db.SaveChangesAsync(); }

    public async Task<List<rotter.Dominio.Entidades.Auditoria>> ObtenerAsync(int pagina, int tamano, string? accion = null) =>
        await Filtrar(_db.Auditorias, accion)
            .OrderByDescending(a => a.FechaAccion)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();

    public async Task<int> TotalAsync(string? accion = null) =>
        await Filtrar(_db.Auditorias, accion).CountAsync();

    private static IQueryable<rotter.Dominio.Entidades.Auditoria> Filtrar(IQueryable<rotter.Dominio.Entidades.Auditoria> query, string? accion) =>
        string.IsNullOrWhiteSpace(accion) ? query : query.Where(a => a.Accion == accion);
}
