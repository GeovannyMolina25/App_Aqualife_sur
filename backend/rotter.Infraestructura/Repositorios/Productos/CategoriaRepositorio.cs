using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Productos;

public class CategoriaRepositorio : ICategoriaRepositorio
{
    private readonly RotterDbContext _db;
    public CategoriaRepositorio(RotterDbContext db) => _db = db;

    public async Task<List<Categoria>> ObtenerTodasAsync(bool soloActivas = true)
    {
        var q = _db.Categorias.AsQueryable();
        if (soloActivas) q = q.Where(c => c.Activo);
        return await q.OrderBy(c => c.Tipo).ThenBy(c => c.Nombre).ToListAsync();
    }
}
