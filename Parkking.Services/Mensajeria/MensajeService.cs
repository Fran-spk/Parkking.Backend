using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Parkking.DTOs.Mensajeria;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services.Mensajeria;

public class MensajeService
{
    private readonly ReciboRepository _recibos;
    private readonly PagoRepository _pagos;
    private readonly IEmailSender _email;
    private readonly MensajeRepository _mensajes;
    private readonly UsuarioRepository _usuarios;
    private readonly IEstacionamientoContext _tenant;
    private readonly EmailOptions _emailOptions;

    public MensajeService(
        ReciboRepository recibos,
        PagoRepository pagos,
        IEmailSender email,
        MensajeRepository mensajes,
        UsuarioRepository usuarios,
        IEstacionamientoContext tenant,
        IOptions<EmailOptions> emailOptions)
    {
        _recibos = recibos;
        _pagos = pagos;
        _email = email;
        _mensajes = mensajes;
        _usuarios = usuarios;
        _tenant = tenant;
        _emailOptions = emailOptions.Value;
    }

    private int TenantId => _tenant.EstacionamientoId;

    /// <summary>Auditoría de mensajes del estacionamiento (no es feature de abono).</summary>
    public List<MensajeDto> Listar(string? tipo = null, string? estado = null, int take = 100) =>
        _mensajes.List(TenantId, tipo, estado, take).Select(Map).ToList();

    public List<MensajeDto> ListarPorAbono(int abonoId, int take = 50)
    {
        if (abonoId <= 0) throw new Exception("Abono inválido.");
        return _mensajes.ListByAbono(TenantId, abonoId, Math.Clamp(take, 1, 200))
            .Select(Map)
            .ToList();
    }

    public List<MensajeDto> ListarPorRecibo(int reciboId, int take = 50)
    {
        if (reciboId <= 0) throw new Exception("Recibo inválido.");
        var recibo = _recibos.GetById(reciboId)
            ?? throw new Exception("Recibo no encontrado.");
        if (recibo.EstacionamientoId != TenantId)
            throw new Exception("Recibo no encontrado.");

        return _mensajes.ListByRecibo(TenantId, reciboId, Math.Clamp(take, 1, 200))
            .Select(Map)
            .ToList();
    }

    public async Task<MensajeDto> EnviarReciboAsync(int reciboId, string? emailOverride = null, CancellationToken ct = default)
    {
        var recibo = _recibos.GetById(reciboId)
            ?? throw new Exception("Recibo no encontrado.");

        if (recibo.EstacionamientoId != TenantId)
            throw new Exception("Recibo no encontrado.");

        if (recibo.Anulado)
            throw new Exception("No se puede enviar un recibo anulado.");

        var abono = recibo.Pago?.Abono
            ?? throw new Exception("El recibo no tiene abono asociado.");
        var cliente = abono.Cliente;
        var datos = _pagos.GetEstacionamiento(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        var destinatario = ResolverDestinatarioCliente(emailOverride, abono, cliente);
        if (string.IsNullOrWhiteSpace(destinatario))
            throw new Exception(
                "No hay email de destino. Cargá el email del cliente, del abono, o indicá uno al enviar.");

        var (fromEmail, fromName) = RemitenteParkking();
        // Reply-To opcional: contacto del estacionamiento (destino ≠ remitente).
        var replyTo = EsEmailUsable(datos.EmailAvisos) ? datos.EmailAvisos.Trim() : null;
        var nombreEst = string.IsNullOrWhiteSpace(datos.Nombre) ? fromName : datos.Nombre.Trim();

        var asunto = $"Recibo {recibo.NumeroFormateado} — {nombreEst}";
        var html = ConstruirHtmlRecibo(recibo, datos);

        var send = await _email.SendAsync(new EmailMessage
        {
            To = destinatario,
            Subject = asunto,
            HtmlBody = html,
            FromEmail = fromEmail,
            FromName = fromName,
            ReplyTo = replyTo,
        }, ct);

        return PersistirYMapear(
            TipoMensaje.Recibo,
            destinatario,
            fromEmail,
            asunto,
            send,
            reciboId: recibo.ReciboId,
            clienteId: cliente?.ClienteId,
            abonoId: abono.AbonoId);
    }

    public async Task<MensajeDto> EnviarReporteAsync(
        string tituloReporte,
        byte[] contenido,
        string fileName,
        string contentType,
        EnviarReporteEmailRequest request,
        CancellationToken ct = default)
    {
        if (contenido is null || contenido.Length == 0)
            throw new Exception("El reporte está vacío.");

        var datos = _pagos.GetEstacionamiento(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        var (destinatario, _) = ResolverDestinatarioReporte(request, datos);
        var nombreEst = string.IsNullOrWhiteSpace(datos.Nombre) ? "Parkking" : datos.Nombre.Trim();
        var (fromEmail, fromName) = RemitenteParkking();

        var asunto = $"{tituloReporte} — {nombreEst}";
        var html = $@"<!DOCTYPE html><html><body style='font-family:Segoe UI,Arial,sans-serif;color:#0f172a;'>
<div style='max-width:560px;margin:0 auto;padding:24px;'>
  <h1 style='font-size:18px;margin:0 0 8px;'>{WebUtility.HtmlEncode(nombreEst)}</h1>
  <p style='margin:0 0 12px;color:#64748b;font-size:14px;'>Adjuntamos el reporte solicitado.</p>
  <p style='margin:0;font-size:14px;'><strong>{WebUtility.HtmlEncode(tituloReporte)}</strong></p>
  <p style='margin:16px 0 0;font-size:12px;color:#94a3b8;'>Mensaje automático de Parkking.</p>
</div></body></html>";

        var send = await _email.SendAsync(new EmailMessage
        {
            To = destinatario,
            Subject = asunto,
            HtmlBody = html,
            FromEmail = fromEmail,
            FromName = fromName,
            Attachments =
            {
                new EmailAttachment
                {
                    FileName = fileName,
                    ContentType = contentType,
                    Content = contenido,
                },
            },
        }, ct);

        return PersistirYMapear(
            TipoMensaje.Reporte,
            destinatario,
            fromEmail,
            asunto,
            send);
    }

    private (string Email, int? UsuarioId) ResolverDestinatarioReporte(
        EnviarReporteEmailRequest request,
        DatosEstacionamiento datos)
    {
        var destino = (request.Destino ?? DestinoReporteEmail.Estacionamiento).Trim().ToLowerInvariant();

        if (destino == DestinoReporteEmail.Usuario)
        {
            if (!request.UsuarioId.HasValue || request.UsuarioId.Value <= 0)
                throw new Exception("Seleccioná un usuario destinatario.");

            if (!_usuarios.TieneAccesoEstacionamiento(request.UsuarioId.Value, TenantId))
                throw new Exception("El usuario no pertenece a este estacionamiento.");

            var usuario = _usuarios.GetById(request.UsuarioId.Value)
                ?? throw new Exception("Usuario no encontrado.");

            if (string.IsNullOrWhiteSpace(usuario.Mail))
                throw new Exception($"El usuario {usuario.Nombre} no tiene email cargado.");

            return (usuario.Mail.Trim(), usuario.USU_ID);
        }

        if (destino != DestinoReporteEmail.Estacionamiento)
            throw new Exception("Destino inválido. Usá 'estacionamiento' o 'usuario'.");

        if (string.IsNullOrWhiteSpace(datos.EmailAvisos))
            throw new Exception(
                "El estacionamiento no tiene email de avisos. Cargalo en Configuración → General.");

        // Destino = casilla del estacionamiento (Parkking → estacionamiento).
        return (datos.EmailAvisos.Trim(), null);
    }

    /// <summary>Único remitente de la app: casilla Parkking (.env Email__*).</summary>
    private (string Email, string Name) RemitenteParkking()
    {
        var email = (_emailOptions.FromEmail ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new Exception("Email__FromEmail no configurado. Revisá Parkking.Api/.env.");

        var name = string.IsNullOrWhiteSpace(_emailOptions.FromName)
            ? "Parkking"
            : _emailOptions.FromName.Trim();
        return (email, name);
    }

    private static string ResolverDestinatarioCliente(string? emailOverride, Abono abono, Cliente? cliente)
    {
        if (!string.IsNullOrWhiteSpace(emailOverride))
            return emailOverride.Trim();
        if (!string.IsNullOrWhiteSpace(abono.Email))
            return abono.Email.Trim();
        if (!string.IsNullOrWhiteSpace(cliente?.Email))
            return cliente!.Email!.Trim();
        return string.Empty;
    }

    /// <summary>Evita placeholders de migration (avisos@parkking.local) y textos sin @.</summary>
    private static bool EsEmailUsable(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return false;
        var e = email.Trim().ToLowerInvariant();
        return !e.EndsWith(".local")
            && !e.EndsWith(".example")
            && !e.Contains("parkking.local")
            && !e.Contains("parkking.example");
    }

    private MensajeDto PersistirYMapear(
        string tipo,
        string destinatario,
        string? remitente,
        string asunto,
        EmailSendResult send,
        int? reciboId = null,
        int? clienteId = null,
        int? abonoId = null)
    {
        var estado = !send.Ok
            ? EstadoMensaje.Error
            : send.Simulated
                ? EstadoMensaje.Simulado
                : EstadoMensaje.Enviado;

        var mensaje = new Mensaje
        {
            EstacionamientoId = TenantId,
            Tipo = tipo,
            Destinatario = destinatario,
            Remitente = remitente,
            Asunto = asunto,
            Estado = estado,
            Error = string.IsNullOrEmpty(send.Error)
                ? null
                : (send.Error.Length <= 1000 ? send.Error : send.Error[..1000]),
            Fecha = DateTime.UtcNow,
            ReciboId = reciboId,
            ClienteId = clienteId,
            AbonoId = abonoId,
        };
        _mensajes.Add(mensaje);
        _mensajes.SaveChanges();

        if (!send.Ok)
            throw new Exception(send.Error ?? "No se pudo enviar el email.");

        return Map(mensaje);
    }

    private static string ConstruirHtmlRecibo(Recibo r, DatosEstacionamiento est)
    {
        var cultura = new CultureInfo("es-AR");
        string Esc(string? s) => WebUtility.HtmlEncode(s ?? "");
        string Money(decimal v) => v.ToString("C", cultura);

        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><body style='font-family:Segoe UI,Arial,sans-serif;color:#0f172a;'>");
        sb.Append("<div style='max-width:560px;margin:0 auto;padding:24px;'>");
        sb.Append($"<h1 style='font-size:20px;margin:0 0 4px;'>{Esc(est.Nombre)}</h1>");
        if (!string.IsNullOrWhiteSpace(est.Direccion))
            sb.Append($"<p style='margin:0 0 16px;color:#64748b;font-size:13px;'>{Esc(est.Direccion)}</p>");
        sb.Append($"<h2 style='font-size:16px;margin:0 0 12px;'>Recibo {Esc(r.NumeroFormateado)}</h2>");
        sb.Append("<table style='width:100%;border-collapse:collapse;font-size:14px;'>");
        Row(sb, "Fecha", r.FechaEmision.ToLocalTime().ToString("dd/MM/yyyy HH:mm", cultura));
        Row(sb, "Cliente", r.ClienteNombre);
        if (!string.IsNullOrWhiteSpace(r.CocherasLabel)) Row(sb, "Cocheras", r.CocherasLabel);
        if (!string.IsNullOrWhiteSpace(r.PatentesLabel)) Row(sb, "Patentes", r.PatentesLabel);
        if (!string.IsNullOrWhiteSpace(r.PeriodosLabel)) Row(sb, "Períodos", r.PeriodosLabel);
        if (!string.IsNullOrWhiteSpace(r.MetodoPagoLabel)) Row(sb, "Método", r.MetodoPagoLabel);
        Row(sb, "Monto", Money(r.Monto));
        if (r.Recargo > 0) Row(sb, "Recargo", Money(r.Recargo));
        Row(sb, "Total", Money(r.Total));
        if (!string.IsNullOrWhiteSpace(r.Observacion)) Row(sb, "Obs.", r.Observacion);
        sb.Append("</table>");
        sb.Append("<p style='margin-top:24px;font-size:12px;color:#94a3b8;'>Mensaje automático de Parkking.</p>");
        sb.Append("</div></body></html>");
        return sb.ToString();

        static void Row(StringBuilder b, string label, string value)
        {
            b.Append("<tr>");
            b.Append($"<td style='padding:6px 8px;color:#64748b;width:35%;border-bottom:1px solid #e2e8f0;'>{WebUtility.HtmlEncode(label)}</td>");
            b.Append($"<td style='padding:6px 8px;border-bottom:1px solid #e2e8f0;'>{WebUtility.HtmlEncode(value)}</td>");
            b.Append("</tr>");
        }
    }

    private static MensajeDto Map(Mensaje m) => new()
    {
        MensajeId = m.MensajeId,
        Tipo = m.Tipo,
        Destinatario = m.Destinatario,
        Remitente = m.Remitente,
        Asunto = m.Asunto,
        Estado = m.Estado,
        Error = m.Error,
        Fecha = m.Fecha,
        ReciboId = m.ReciboId,
        ClienteId = m.ClienteId,
        AbonoId = m.AbonoId,
        Simulado = m.Estado == EstadoMensaje.Simulado,
    };
};
