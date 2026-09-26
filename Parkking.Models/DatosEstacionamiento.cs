using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Datos operativos y reglas de cobro del estacionamiento (tenant).
/// </summary>
[Table("Estacionamientos")]
public class DatosEstacionamiento
{
    public int EstacionamientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    /// <summary>Nombre del locador en el contrato (titular). Si vacío, el PDF usa Nombre.</summary>
    [MaxLength(150)]
    public string? LocadorNombre { get; set; }

    /// <summary>DNI / CUIT del locador para el contrato.</summary>
    [MaxLength(30)]
    public string? LocadorDocumento { get; set; }

    /// <summary>Domicilio del locador. Si vacío, el PDF usa Direccion del inmueble.</summary>
    [MaxLength(200)]
    public string? LocadorDomicilio { get; set; }

    /// <summary>Día del mes en que vencen las cuotas mensuales (1–31).</summary>
    public int DiaVencimientoAbono { get; set; }

    /// <summary>Si true, se puede sugerir/aplicar mora según <see cref="PorcentajeRecargo"/>.</summary>
    public bool AplicaRecargo { get; set; }

    /// <summary>% de mora sobre el monto (solo si <see cref="AplicaRecargo"/>).</summary>
    public decimal PorcentajeRecargo { get; set; }

    /// <summary>Baja lógica del estacionamiento.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Si true, el front puede ofrecer/imprimir el recibo automáticamente al cobrar.
    /// El recibo se genera igual en backend; esto solo controla la UX de impresión.
    /// </summary>
    public bool ImprimirReciboAlCobrar { get; set; } = true;

    /// <summary>
    /// Si true, al registrar un pago se envía el recibo por email al cliente
    /// (Abono.Email ?? Cliente.Email). Fallo de mail no revierte el cobro.
    /// </summary>
    public bool EnviarReciboPorEmail { get; set; }

    /// <summary>
    /// Solo afecta el PDF de contrato: incluye la cláusula de seguro obligatorio.
    /// No cambia validaciones ni pantallas de abonos/cobros.
    /// </summary>
    public bool ContratoSeguroObligatorio { get; set; } = true;

    /// <summary>
    /// Plazo genérico del contrato en meses (todas las cocheras del estacionamiento).
    /// Solo se usa al generar el PDF; el abono sigue siendo vigente hasta la baja.
    /// </summary>
    public int ContratoPlazoMeses { get; set; } = 12;

    /// <summary>
    /// Si true, al crear un abono el front descarga automáticamente el PDF de contrato.
    /// No persiste el PDF: se regenera bajo demanda.
    /// </summary>
    public bool GenerarContratoAlCrearAbono { get; set; } = true;

    /// <summary>
    /// Email de contacto del estacionamiento (destino de avisos/reportes Parkking → estacionamiento).
    /// No es el remitente SMTP: todos los mails salen desde la casilla Parkking (.env).
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string EmailAvisos { get; set; } = string.Empty;

    /// <summary>
    /// Recargo sugerido por mora. 0 si no aplica, o si aún no venció el período.
    /// No se debe persistir automáticamente: el operador lo confirma al cobrar.
    /// </summary>
    public decimal CalcularRecargoSugerido(
        decimal monto,
        DateOnly periodoInicio,
        DateOnly hoy,
        PeriodicidadCobro periodicidad)
    {
        if (!AplicaRecargo || PorcentajeRecargo <= 0 || monto <= 0)
            return 0;

        var dia = Math.Clamp(DiaVencimientoAbono, 1, DateTime.DaysInMonth(periodoInicio.Year, periodoInicio.Month));
        var vencimiento = new DateOnly(periodoInicio.Year, periodoInicio.Month, dia);

        if (periodicidad == PeriodicidadCobro.Quincenal && periodoInicio.Day >= 16)
        {
            var finMes = DateTime.DaysInMonth(periodoInicio.Year, periodoInicio.Month);
            dia = Math.Clamp(DiaVencimientoAbono, 16, finMes);
            vencimiento = new DateOnly(periodoInicio.Year, periodoInicio.Month, dia);
        }

        if (hoy <= vencimiento)
            return 0;

        return Math.Round(monto * PorcentajeRecargo / 100m, 2);
    }
}
