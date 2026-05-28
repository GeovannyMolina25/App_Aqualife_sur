using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface IUsuarioRepositorio
{
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<Usuario?> ObtenerPorTelefonoAsync(string telefono);
    Task<Usuario?> ObtenerPorIdAsync(int id);
    Task<List<Usuario>> ObtenerTodosAsync(int pagina, int tamano);
    Task<int> TotalAsync();
    Task<Usuario> CrearAsync(Usuario usuario);
    Task<Usuario> ActualizarAsync(Usuario usuario);
    Task<bool> ExisteEmailAsync(string email);
    Task CambiarRolAsync(int usuarioId, int nuevoRolId);
}
