using Parkking.Models.Enums;

namespace Parkking.Models.Cobro;

/// <summary>
/// Anual: ciclo de 12 meses anclado al mes de inicio (no ene–dic fijo).
/// Primer período a escala del ciclo (sin desglose mes-a-mes).
/// </summary>
public sealed class AnualStrategy : IPeriodicidadStrategy
{
    private readonly AgrupacionMensualStrategy _ciclo = new(12);

    public int MesesPorCiclo => 12;

    public PeriodoCobro CicloQueContiene(DateOnly fecha, DateOnly anclaMes) =>
        _ciclo.CicloQueContiene(fecha, anclaMes);

    public PeriodoCobro SiguienteCiclo(PeriodoCobro actual, DateOnly anclaMes) =>
        _ciclo.SiguienteCiclo(actual, anclaMes);

    public IEnumerable<PeriodoCobro> Enumerar(DateOnly desdeCobro, DateOnly hasta, DateOnly anclaMes) =>
        _ciclo.Enumerar(desdeCobro, hasta, anclaMes);

    public PrimerPeriodoResult ResolverPrimerPeriodo(
        DateOnly fechaInicio,
        PoliticaPrimerPeriodo politica,
        decimal montoBaseCiclo,
        DateOnly anclaMes)
    {
        if (montoBaseCiclo < 0)
            throw new ArgumentOutOfRangeException(nameof(montoBaseCiclo));

        politica = AgrupacionMensualStrategy.NormalizarPolitica(fechaInicio, politica);
        var ancla = new DateOnly(anclaMes.Year, anclaMes.Month, 1);
        var ciclo = CicloQueContiene(fechaInicio, ancla);

        return politica switch
        {
            PoliticaPrimerPeriodo.Prorratear => ProrratearCiclo(fechaInicio, ciclo, montoBaseCiclo),
            PoliticaPrimerPeriodo.OmitirMesEntrante => new PrimerPeriodoResult(
                SiguienteCiclo(ciclo, ancla),
                Redondear(montoBaseCiclo),
                FueProrrateado: false),
            _ => new PrimerPeriodoResult(
                new PeriodoCobro(fechaInicio, ciclo.Fin),
                Redondear(montoBaseCiclo),
                FueProrrateado: false),
        };
    }

    private static PrimerPeriodoResult ProrratearCiclo(DateOnly fechaInicio, PeriodoCobro ciclo, decimal baseCiclo)
    {
        var diasCiclo = ciclo.Dias;
        var diasRestantes = ciclo.Fin.DayNumber - fechaInicio.DayNumber + 1;
        var monto = Redondear(baseCiclo * diasRestantes / diasCiclo);
        return new PrimerPeriodoResult(
            new PeriodoCobro(fechaInicio, ciclo.Fin),
            monto,
            FueProrrateado: true);
    }

    private static decimal Redondear(decimal v) =>
        Math.Round(v, 0, MidpointRounding.AwayFromZero);
}
