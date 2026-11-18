using rotter.Dominio.Entidades;

namespace rotter.Dominio.Interfaces.Repositorios;

public interface ICategoriaRepositorio
{
    Task<List<Categoria>> ObtenerTodasAsync(bool soloActivas = true);
}
