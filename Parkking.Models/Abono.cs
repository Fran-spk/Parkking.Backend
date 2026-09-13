using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Contrato de abono: agrupa plazas (cocheras) y vehículos habilitados.
/// </summary>
public class Abono : IMultiTenant
{
    [Key]
    public int AbonoId { get; set; }

    [ForeignKey(nameof(Estacionamiento))]
    public int EstacionamientoId { get; set; }
    public DatosEstacionamiento? Estacionamiento { get; set; }

    [ForeignKey(nameof(Cliente))]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [MaxLength(100)]
    public string? Cobrador { get; set; }

    [Required]
    public DateOnly FechaInicio { get; set; }

    [Required]
    public DateOnly FechaInicioCobro { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioAcordado { get; set; }

    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;

    public bool Activo { get; set; } = true;

    public ICollection<AbonoPlaza> Plazas { get; set; } = new List<AbonoPlaza>();
    public ICollection<AbonoVehiculo> AbonoVehiculos { get; set; } = new List<AbonoVehiculo>();
    public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public bool TieneDeuda() => ObtenerPrimerPeriodoImpago() != null;

    /// <summary>Compat: primer período impago (inicio del período).</summary>
    public DateOnly? ObtenerPrimerMesImpago() => ObtenerPrimerPeriodoImpago();

    public DateOnly? ObtenerPrimerPeriodoImpago()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        foreach (var (inicio, _) in PeriodicidadHelper.EnumerarPeriodos(FechaInicioCobro, hoy, PeriodicidadCobro))
        {
            var cuota = Cuotas.FirstOrDefault(c =>
                c.Estado != EstadoCuota.Anulada &&
                c.PeriodoInicio == inicio);

            if (cuota == null || cuota.Estado is EstadoCuota.Pendiente or EstadoCuota.Parcial)
                return inicio;
        }

        return null;
    }

    public int ObtenerDiasAtraso()
    {
        var primer = ObtenerPrimerPeriodoImpago();
        if (primer == null)
            return 0;

        return (DateTime.Today - primer.Value.ToDateTime(TimeOnly.MinValue)).Days;
    }

    public decimal CalcularMontoPeriodo()
    {
        if (PrecioAcordado.HasValue)
            return PrecioAcordado.Value;

        throw new InvalidOperationException(
            $"El abono {AbonoId} no tiene PrecioAcordado. " +
            "Usar AbonoPrecioService.Resolver para sumar tarifas de vehículos fijos.");
    }

    [Obsolete("Usar CalcularMontoPeriodo")]
    public decimal CalcularMontoMensual() => CalcularMontoPeriodo();
}
