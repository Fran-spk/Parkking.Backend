using System.ComponentModel.DataAnnotations.Schema;
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

    /// <summary>Día del mes en que vencen las cuotas mensuales (1–31).</summary>
    public int DiaVencimientoAbono { get; set; }

    /// <summary>Si true, se puede sugerir/aplicar mora según <see cref="PorcentajeRecargo"/>.</summary>
    public bool AplicaRecargo { get; set; }

    /// <summary>% de mora sobre el monto (solo si <see cref="AplicaRecargo"/>).</summary>
    public decimal PorcentajeRecargo { get; set; }

    /// <summary>
    /// Día del mes a partir del cual el ingreso genera 1ª cuota prorrateada
    /// (solo periodicidad mensual). Null = no prorratear nunca.
    /// </summary>
    public int? DiasUmbralProporcional { get; set; }

    /// <summary>Baja lógica del estacionamiento.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Si true, el front puede ofrecer/imprimir el recibo automáticamente al cobrar.
    /// El recibo se genera igual en backend; esto solo controla la UX de impresión.
    /// </summary>
    public bool ImprimirReciboAlCobrar { get; set; } = true;

    /// <summary>
    /// Importe de la primera cuota: base o prorrateo según umbral.
    /// Nunca incluye recargo (la mora se calcula al cobrar, si corresponde).
    /// </summary>
    public decimal CalcularMontoPrimeraCuota(
        decimal montoBase,
        DateOnly fechaIngreso,
        DateOnly periodoInicioCobro,
        PeriodicidadCobro periodicidad)
    {
        if (montoBase <= 0)
            return 0;

        if (periodicidad != PeriodicidadCobro.Mensual)
            return montoBase;

        var (periodoIngreso, _) = PeriodicidadHelper.PeriodoQueContiene(fechaIngreso, periodicidad);
        if (periodoIngreso != periodoInicioCobro)
            return montoBase;

        if (DiasUmbralProporcional is null)
            return montoBase;

        var dia = fechaIngreso.Day;
        if (dia <= DiasUmbralProporcional.Value)
            return montoBase;

        var diasMes = DateTime.DaysInMonth(fechaIngreso.Year, fechaIngreso.Month);
        var diasRestantes = diasMes - dia + 1;
        return Math.Round((montoBase / diasMes) * diasRestantes, 0);
    }

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
