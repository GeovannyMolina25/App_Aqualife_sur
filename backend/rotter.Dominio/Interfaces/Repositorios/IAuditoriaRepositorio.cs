using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface IAuditoriaRepositorio
{
    Task RegistrarAsync(Auditoria auditoria);
    Task<List<Auditoria>> ObtenerAsync(int pagina, int tamano);
}
