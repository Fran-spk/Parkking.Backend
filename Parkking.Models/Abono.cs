using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Cobro;
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

    /// <summary>
    /// Email de contacto del abono para avisos (recibo, etc.).
    /// Si es null, se usa el email del cliente.
    /// </summary>
    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    public DateOnly FechaInicio { get; set; }

    [Required]
    public DateOnly FechaInicioCobro { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioAcordado { get; set; }

    public PeriodicidadCobro PeriodicidadCobro { get; set; } = PeriodicidadCobro.Mensual;

    /// <summary>Tratamiento del mes de alta (prorratear / omitir mes entrante / completo).</summary>
    public PoliticaPrimerPeriodo PoliticaPrimerPeriodo { get; set; } = PoliticaPrimerPeriodo.PeriodoCompleto;

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
        var strategy = PeriodicidadStrategyFactory.For(PeriodicidadCobro);
        var ancla = PeriodicidadStrategyFactory.AnclaDesdeFechaInicio(FechaInicio);
        foreach (var periodo in strategy.Enumerar(FechaInicioCobro, hoy, ancla))
        {
            var cuota = Cuotas.FirstOrDefault(c =>
                c.Estado != EstadoCuota.Anulada &&
                c.PeriodoInicio == periodo.Inicio);

            if (cuota == null || cuota.Estado is EstadoCuota.Pendiente or EstadoCuota.Parcial)
                return periodo.Inicio;
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
