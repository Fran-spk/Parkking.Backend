using Parkking.Models.Enums;

namespace Parkking.Models.Cobro;

/// <summary>
/// Strategy por familia de periodicidad: ciclos + primer período.
/// </summary>
public interface IPeriodicidadStrategy
{
    /// <summary>Meses por ciclo (0 = quincenal).</summary>
    int MesesPorCiclo { get; }

    PeriodoCobro CicloQueContiene(DateOnly fecha, DateOnly anclaMes);

    PeriodoCobro SiguienteCiclo(PeriodoCobro actual, DateOnly anclaMes);

    IEnumerable<PeriodoCobro> Enumerar(DateOnly desdeCobro, DateOnly hasta, DateOnly anclaMes);

    PrimerPeriodoResult ResolverPrimerPeriodo(
        DateOnly fechaInicio,
        PoliticaPrimerPeriodo politica,
        decimal montoBaseCiclo,
        DateOnly anclaMes);
}
