namespace rotter.Dominio.Interfaces.Servicios
{
    public interface IEmailServicio
    {
        Task EnviarAsync(
        string para,
        string asunto,
        string html
    );
    }
}
