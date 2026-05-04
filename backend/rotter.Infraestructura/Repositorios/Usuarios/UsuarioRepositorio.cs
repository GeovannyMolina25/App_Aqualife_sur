using Microsoft.EntityFrameworkCore;
using rotter.Dominio.Entidades;
using rotter.Dominio.Interfaces.Repositorios;
using rotter.Infraestructura.Data;

namespace rotter.Infraestructura.Repositorios.Usuarios;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly RotterDbContext _db;
    public UsuarioRepositorio(RotterDbContext db) => _db = db;

    public async Task<Usuario?> ObtenerPorEmailAsync(string email) =>
        await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<Usuario?> ObtenerPorIdAsync(int id) =>
        await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<Usuario>> ObtenerTodosAsync(int pagina, int tamano) =>
        await _db.Usuarios.Include(u => u.Rol)
            .OrderBy(u => u.Nombre)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .ToListAsync();

    public async Task<int> TotalAsync() => await _db.Usuarios.CountAsync();

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        usuario.Email = usuario.Email.ToLower();
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> ActualizarAsync(Usuario usuario)
    {
        usuario.FechaModificacion = DateTime.UtcNow;
        _db.Usuarios.Update(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> ExisteEmailAsync(string email) =>
        await _db.Usuarios.AnyAsync(u => u.Email == email.ToLower());

    public async Task CambiarRolAsync(int usuarioId, int nuevoRolId)
    {
        var usuario = await _db.Usuarios.FindAsync(usuarioId);
        if (usuario is null) return;
        usuario.RolId = nuevoRolId;
        usuario.FechaModificacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
