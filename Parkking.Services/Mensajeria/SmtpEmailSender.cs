using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Parkking.Services.Mensajeria;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message.To))
            return new EmailSendResult(false, false, "Destinatario vacío.");

        // Único remitente = casilla Parkking (.env). Destinos van en To / Reply-To.
        var fromEmail = (_options.FromEmail ?? string.Empty).Trim();
        var fromName = string.IsNullOrWhiteSpace(_options.FromName) ? "Parkking" : _options.FromName.Trim();

        if (!_options.Enabled)
        {
            var adjuntos = message.Attachments.Count == 0
                ? "(sin adjuntos)"
                : string.Join(", ", message.Attachments.Select(a => a.FileName));
            _logger.LogWarning(
                "Email deshabilitado (Email:Enabled=false). Simulado → {To} | {Subject} | {Adjuntos}",
                message.To, message.Subject, adjuntos);
            return new EmailSendResult(true, true);
        }

        try
        {
            var mime = new MimeMessage();
            if (string.IsNullOrWhiteSpace(fromEmail) || !fromEmail.Contains('@'))
                return new EmailSendResult(false, false, "FromEmail SMTP inválido. Revisá Email__FromEmail en .env.");

            mime.From.Add(new MailboxAddress(fromName, fromEmail));
            if (!MailboxAddress.TryParse(message.To.Trim(), out var toMailbox))
                return new EmailSendResult(false, false, $"Destinatario inválido: {message.To}");
            mime.To.Add(toMailbox);
            if (!string.IsNullOrWhiteSpace(message.ReplyTo)
                && MailboxAddress.TryParse(message.ReplyTo.Trim(), out var replyMailbox))
                mime.ReplyTo.Add(replyMailbox);
            mime.Subject = message.Subject;

            var builder = new BodyBuilder { HtmlBody = message.HtmlBody };
            foreach (var att in message.Attachments)
            {
                if (att.Content is null || att.Content.Length == 0) continue;
                var ctPart = ContentType.Parse(
                    string.IsNullOrWhiteSpace(att.ContentType)
                        ? "application/octet-stream"
                        : att.ContentType);
                builder.Attachments.Add(att.FileName, att.Content, ctPart);
            }
            mime.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var secure = _options.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_options.Host, _options.Port, secure, ct);

            var user = (_options.UserName ?? string.Empty).Trim();
            // App passwords de Gmail suelen pegarse con espacios; SMTP autentica sin ellos.
            var pass = (_options.Password ?? string.Empty).Replace(" ", string.Empty);
            if (!string.IsNullOrWhiteSpace(user))
                await client.AuthenticateAsync(user, pass, ct);

            await client.SendAsync(mime, ct);
            await client.DisconnectAsync(true, ct);
            return new EmailSendResult(true, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando mail a {To}", message.To);
            return new EmailSendResult(false, false, ex.Message);
        }
    }
}
