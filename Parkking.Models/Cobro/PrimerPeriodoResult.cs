namespace Parkking.Models.Cobro;

public sealed record PrimerPeriodoResult(
    PeriodoCobro Periodo,
    decimal Monto,
    bool FueProrrateado);
