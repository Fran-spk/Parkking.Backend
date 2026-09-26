using Parkking.Models.Enums;

namespace Parkking.Models.Cobro;

/// <summary>Quincenas calendario: 1–15 y 16–fin de mes.</summary>
public sealed class QuincenalStrategy : IPeriodicidadStrategy
{
    public int MesesPorCiclo => 0;

    public PeriodoCobro CicloQueContiene(DateOnly fecha, DateOnly anclaMes) => Slot(fecha);

    public PeriodoCobro SiguienteCiclo(PeriodoCobro actual, DateOnly anclaMes)
    {
        var next = actual.Fin.AddDays(1);
        return Slot(next);
    }

    public IEnumerable<PeriodoCobro> Enumerar(DateOnly desdeCobro, DateOnly hasta, DateOnly anclaMes)
    {
        var slot = Slot(desdeCobro);
        var limite = Slot(hasta).Inicio;

        var primero = new PeriodoCobro(desdeCobro, slot.Fin);
        if (primero.Inicio <= primero.Fin && primero.Inicio <= hasta)
            yield return primero;

        var cursor = SiguienteCiclo(slot, anclaMes);
        while (cursor.Inicio <= limite)
        {
            yield return cursor;
            cursor = SiguienteCiclo(cursor, anclaMes);
        }
    }

    public PrimerPeriodoResult ResolverPrimerPeriodo(
        DateOnly fechaInicio,
        PoliticaPrimerPeriodo politica,
        decimal montoBaseCiclo,
        DateOnly anclaMes)
    {
        if (montoBaseCiclo < 0)
            throw new ArgumentOutOfRangeException(nameof(montoBaseCiclo));

        politica = AgrupacionMensualStrategy.NormalizarPolitica(fechaInicio, politica);
        var slot = Slot(fechaInicio);

        return politica switch
        {
            PoliticaPrimerPeriodo.Prorratear => Prorratear(fechaInicio, slot, montoBaseCiclo),
            PoliticaPrimerPeriodo.OmitirMesEntrante => new PrimerPeriodoResult(
                SiguienteCiclo(slot, anclaMes),
                Redondear(montoBaseCiclo),
                FueProrrateado: false),
            _ => new PrimerPeriodoResult(
                new PeriodoCobro(fechaInicio, slot.Fin),
                Redondear(montoBaseCiclo),
                FueProrrateado: false),
        };
    }

    private static PrimerPeriodoResult Prorratear(DateOnly fechaInicio, PeriodoCobro slot, decimal baseSlot)
    {
        var diasSlot = slot.Dias;
        var diasRestantes = slot.Fin.DayNumber - fechaInicio.DayNumber + 1;
        var monto = Redondear(baseSlot * diasRestantes / diasSlot);
        return new PrimerPeriodoResult(
            new PeriodoCobro(fechaInicio, slot.Fin),
            monto,
            FueProrrateado: true);
    }

    private static PeriodoCobro Slot(DateOnly fecha)
    {
        if (fecha.Day <= 15)
        {
            return new PeriodoCobro(
                new DateOnly(fecha.Year, fecha.Month, 1),
                new DateOnly(fecha.Year, fecha.Month, 15));
        }

        var fin = new DateOnly(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month));
        return new PeriodoCobro(new DateOnly(fecha.Year, fecha.Month, 16), fin);
    }

    private static decimal Redondear(decimal v) =>
        Math.Round(v, 0, MidpointRounding.AwayFromZero);
}
