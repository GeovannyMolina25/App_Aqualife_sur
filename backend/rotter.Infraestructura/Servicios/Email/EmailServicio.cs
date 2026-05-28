using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using rotter.Dominio.Interfaces.Servicios;

namespace rotter.Infraestructura.Servicios;

public class EmailServicio : IEmailServicio
{
    private readonly IConfiguration _config;

    public EmailServicio(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarAsync(
        string para,
        string asunto,
        string html
    )
    {
        var email = new MimeMessage();

        email.From.Add(
            MailboxAddress.Parse(
                _config["EmailSettings:Email"]
            )
        );

        email.To.Add(
            MailboxAddress.Parse(para)
        );

        email.Subject = asunto;

        email.Body = new TextPart("html")
        {
            Text = html
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _config["EmailSettings:Host"],
            int.Parse(
                _config["EmailSettings:Port"]!
            ),
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await smtp.AuthenticateAsync(
            _config["EmailSettings:Email"],
            _config["EmailSettings:Password"]
        );

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}