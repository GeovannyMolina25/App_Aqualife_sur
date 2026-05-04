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

    public async Task<List<rotter.Dominio.Entidades.Auditoria>> ObtenerAsync(int pagina, int tamano) =>
        await _db.Auditorias.OrderByDescending(a => a.FechaAccion)
            .Skip((pagina - 1) * tamano).Take(tamano).ToListAsync();
}
