namespace rotter.Dominio.DTOs.Comun;

public record RespuestaDto<T>
{
    public bool Exito { get; init; }
    public string Mensaje { get; init; } = string.Empty;
    public T? Datos { get; init; }
    public List<string> Errores { get; init; } = new();
    public int StatusCode { get; init; }

    public static RespuestaDto<T> Ok(T datos, string mensaje = "Operación exitosa") =>
        new() { Exito = true, Datos = datos, Mensaje = mensaje, StatusCode = 200 };

    public static RespuestaDto<T> Fallo(string mensaje, int status = 400, List<string>? errores = null) =>
        new() { Exito = false, Mensaje = mensaje, StatusCode = status, Errores = errores ?? new() };
}
