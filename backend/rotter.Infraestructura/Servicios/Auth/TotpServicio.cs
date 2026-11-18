using OtpNet;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Auth;

public class TotpServicio : ITotpServicio
{
    private const string Emisor = "AqualifeSur";

    public string GenerarSecreto() => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public string ObtenerUriConfiguracion(string secreto, string email) =>
        $"otpauth://totp/{Emisor}:{Uri.EscapeDataString(email)}?secret={secreto}&issuer={Emisor}&digits=6&period=30";

    public bool VerificarCodigo(string secreto, string codigo)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secreto));
        return totp.VerifyTotp(codigo, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
