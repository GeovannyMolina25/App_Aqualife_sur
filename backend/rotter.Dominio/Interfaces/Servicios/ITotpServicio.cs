namespace rotter.Dominio.Interfaces.Servicios;

public interface ITotpServicio
{
    string GenerarSecreto();
    string ObtenerUriConfiguracion(string secreto, string email);
    bool VerificarCodigo(string secreto, string codigo);
}
