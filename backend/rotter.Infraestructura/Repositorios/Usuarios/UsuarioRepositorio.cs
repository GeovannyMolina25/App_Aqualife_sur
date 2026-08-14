using DocumentFormat.OpenXml.InkML;
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

    public async Task<Usuario?> ObtenerPorTelefonoAsync(
    string telefono
)
    {
        return await _db.Usuarios
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Telefono == telefono);
    }
    public async Task<Usuario?> ObtenerPorIdAsync(int id) =>
        await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<Usuario>> ObtenerTodosAsync(int pagina, int tamano, string? busqueda = null) =>
        await FiltrarPorBusqueda(_db.Usuarios.Include(u => u.Rol), busqueda)
            .OrderBy(u => u.Nombre)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .ToListAsync();

    public async Task<int> TotalAsync(string? busqueda = null) =>
        await FiltrarPorBusqueda(_db.Usuarios, busqueda).CountAsync();

    private static IQueryable<Usuario> FiltrarPorBusqueda(IQueryable<Usuario> query, string? busqueda)
    {
        if (string.IsNullOrWhiteSpace(busqueda)) return query;
        var texto = busqueda.Trim();
        return query.Where(u =>
            EF.Functions.Like(u.Nombre, $"%{texto}%") ||
            EF.Functions.Like(u.Apellido, $"%{texto}%") ||
            EF.Functions.Like(u.Email, $"%{texto}%") ||
            EF.Functions.Like(u.Direccion, $"%{texto}%"));
    }

    public async Task<Usuario> CrearAsync(Usuario usuario)
    {
        usuario.Email = usuario.Email.ToLower();
        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> ActualizarAsync(Usuario usuario)
    {
        usuario.FechaModificacion = DateTime.Now;
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
        usuario.FechaModificacion = DateTime.Now;
        await _db.SaveChangesAsync();
    }

    // Update condicional atómico: no hace nada si el usuario no tiene la promoción del
    // 7º botellón activa o ya se le entregó, sin necesidad de leerlo primero en memoria.
    public async Task IncrementarRecargaSeptimoAsync(int usuarioId)
    {
        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE Usuarios SET RecargasParaSeptimo = RecargasParaSeptimo + 1
            WHERE Id = {usuarioId} AND PremioBienvenida = 'SeptimoBotellonGratis' AND PremioBienvenidaEntregado = 0");
    }
}
