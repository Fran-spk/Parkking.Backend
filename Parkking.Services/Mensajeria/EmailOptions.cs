namespace Parkking.Services.Mensajeria;

/// <summary>
/// Casilla SMTP única de Parkking (la app). Es el único remitente de todos los mails:
/// recibos a clientes, reportes a usuarios o al estacionamiento, etc.
/// Los emails de Cliente / Usuario / EmailAvisos son solo destinos (To), nunca From.
/// Configurar vía .env: Email__*.
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Si false, no se conecta a SMTP: el envío queda registrado como Simulado.</summary>
    public bool Enabled { get; set; }

    public string Host { get; set; } = "smtp.example.com";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;

    /// <summary>Usuario SMTP = casilla Parkking (ej. parkking@gmail.com).</summary>
    public string UserName { get; set; } = "avisos@parkking.example";
    public string Password { get; set; } = "CAMBIAR_PASSWORD";

    /// <summary>From de todos los envíos (misma casilla Parkking).</summary>
    public string FromEmail { get; set; } = "avisos@parkking.example";
    public string FromName { get; set; } = "Parkking";
}

public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[] Content { get; set; } = Array.Empty<byte>();
}

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    /// <summary>Opcional. Si viene vacío, SmtpEmailSender usa EmailOptions (casilla Parkking).</summary>
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    /// <summary>Opcional. P.ej. EmailAvisos del estacionamiento para que el cliente pueda responder ahí.</summary>
    public string? ReplyTo { get; set; }
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public interface IEmailSender
{
    /// <summary>Envía el mail. Si SMTP está deshabilitado, no lanza y retorna Simulated=true.</summary>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public record EmailSendResult(bool Ok, bool Simulated, string? Error = null);
