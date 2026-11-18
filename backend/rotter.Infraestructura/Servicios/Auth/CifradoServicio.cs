using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios.Auth;

public class CifradoServicio : ICifradoServicio
{
    private const int TamanoNonce = 12;
    private const int TamanoTag   = 16;

    private readonly byte[] _clave;

    public CifradoServicio(IConfiguration config)
    {
        var claveBase64 = config["Seguridad:ClaveCifrado"];
        if (string.IsNullOrWhiteSpace(claveBase64))
            throw new InvalidOperationException(
                "Seguridad:ClaveCifrado no está configurado. Defínelo con 'dotnet user-secrets set \"Seguridad:ClaveCifrado\" \"...\"' en desarrollo o la variable de entorno Seguridad__ClaveCifrado en producción.");

        _clave = Convert.FromBase64String(claveBase64);
    }

    public string Cifrar(string textoPlano)
    {
        var nonce   = RandomNumberGenerator.GetBytes(TamanoNonce);
        var texto   = Encoding.UTF8.GetBytes(textoPlano);
        var cifrado = new byte[texto.Length];
        var tag     = new byte[TamanoTag];

        using var aes = new AesGcm(_clave, TamanoTag);
        aes.Encrypt(nonce, texto, cifrado, tag);

        var resultado = new byte[nonce.Length + tag.Length + cifrado.Length];
        Buffer.BlockCopy(nonce, 0, resultado, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, resultado, nonce.Length, tag.Length);
        Buffer.BlockCopy(cifrado, 0, resultado, nonce.Length + tag.Length, cifrado.Length);
        return Convert.ToBase64String(resultado);
    }

    public string Descifrar(string textoCifrado)
    {
        var datos   = Convert.FromBase64String(textoCifrado);
        var nonce   = datos.AsSpan(0, TamanoNonce);
        var tag     = datos.AsSpan(TamanoNonce, TamanoTag);
        var cifrado = datos.AsSpan(TamanoNonce + TamanoTag);
        var texto   = new byte[cifrado.Length];

        using var aes = new AesGcm(_clave, TamanoTag);
        aes.Decrypt(nonce, cifrado, tag, texto);
        return Encoding.UTF8.GetString(texto);
    }
}
