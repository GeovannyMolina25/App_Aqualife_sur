namespace rotter.Dominio.Interfaces.Servicios;

public interface IAuditoriaServicio
{
    Task RegistrarAsync(
        string accion,
        string entidad,
        string? entidadId = null,
        object? datosAnteriores = null,
        object? datosNuevos = null,
        bool exitoso = true,
        string? mensajeError = null);
}
