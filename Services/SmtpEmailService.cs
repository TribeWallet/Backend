using System.Net.Mail;

namespace TribeWallet.Services;

/// <summary>
/// Envia e-mail por SMTP. Em desenvolvimento aponta para o Mailpit (localhost:1025, sem TLS e sem
/// autenticação); em produção basta trocar SMTP_HOST/PORT/USER/PASSWORD/SSL no .env.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task EnviarAsync(string para, string assunto, string corpoHtml)
    {
        var host = _configuration["SMTP_HOST"] ?? "localhost";
        var porta = int.Parse(_configuration["SMTP_PORT"] ?? "1025");
        var remetente = _configuration["SMTP_FROM"] ?? "no-reply@tribewallet.local";

        using var client = new SmtpClient(host, porta)
        {
            EnableSsl = _configuration.GetValue("SMTP_SSL", false)
        };

        var usuario = _configuration["SMTP_USER"];
        if (!string.IsNullOrEmpty(usuario))
            client.Credentials = new System.Net.NetworkCredential(usuario, _configuration["SMTP_PASSWORD"]);

        using var mensagem = new MailMessage(remetente, para, assunto, corpoHtml) { IsBodyHtml = true };
        await client.SendMailAsync(mensagem);
    }
}
