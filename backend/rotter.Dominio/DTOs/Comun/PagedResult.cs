namespace rotter.Dominio.DTOs.Comun;

public record PagedResult<T>(
    List<T> Items,
    int Total,
    int Pagina,
    int TamañoPagina,
    int TotalPaginas
);
