using Parkking.Models.Enums;

namespace Parkking.Models.Cobro;

/// <summary>
/// Mensual (N=1), bimestral (2), trimestral (3), semestral (6).
/// Lógica única: prorratear / omitir mes entrante / completar el mes de alta; resto del ciclo entero.
/// </summary>
public sealed class AgrupacionMensualStrategy : IPeriodicidadStrategy
{
    /// <summary>Días 1 y 2 del mes no se prorratean.</summary>
    public const int DiaMaximoSinProrrateo = 2;

    private readonly int _n;

    public AgrupacionMensualStrategy(int mesesPorCiclo)
    {
        if (mesesPorCiclo is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(mesesPorCiclo));
        _n = mesesPorCiclo;
    }

    public int MesesPorCiclo => _n;

    public PeriodoCobro CicloQueContiene(DateOnly fecha, DateOnly anclaMes)
    {
        var ancla = PrimerDiaMes(anclaMes);
        var monthsFromAnchor = (fecha.Year - ancla.Year) * 12 + (fecha.Month - ancla.Month);
        var cycleIndex = FloorDiv(monthsFromAnchor, _n);
        var start = ancla.AddMonths(cycleIndex * _n);
        return CicloDesdeInicio(start);
    }

    public PeriodoCobro SiguienteCiclo(PeriodoCobro actual, DateOnly anclaMes)
    {
        var nextStart = PrimerDiaMes(actual.Inicio).AddMonths(_n);
        return CicloDesdeInicio(nextStart);
    }

    public IEnumerable<PeriodoCobro> Enumerar(DateOnly desdeCobro, DateOnly hasta, DateOnly anclaMes)
    {
        // Primera cuota puede empezar a mitad de mes: el primer período lo arma el caller
        // con FechaInicioCobro. Acá enumeramos ciclos naturales desde el ciclo que contiene desdeCobro,
        // pero si desdeCobro no es día 1, el primer yield debe respetar ese inicio.
        var ciclo = CicloQueContiene(desdeCobro, anclaMes);
        var limite = CicloQueContiene(hasta, anclaMes).Inicio;

        var primero = new PeriodoCobro(desdeCobro, ciclo.Fin);
        if (primero.Inicio <= primero.Fin && primero.Inicio <= hasta)
            yield return primero;

        var cursor = SiguienteCiclo(ciclo, anclaMes);
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

        politica = NormalizarPolitica(fechaInicio, politica);
        var ancla = PrimerDiaMes(anclaMes);
        var ciclo = CicloQueContiene(fechaInicio, ancla);
        var aporteMes = montoBaseCiclo / _n;

        return politica switch
        {
            PoliticaPrimerPeriodo.Prorratear => ResolverProrratear(fechaInicio, ciclo, aporteMes, montoBaseCiclo),
            PoliticaPrimerPeriodo.OmitirMesEntrante => ResolverOmitirMesEntrante(fechaInicio, ciclo, ancla, aporteMes, montoBaseCiclo),
            _ => new PrimerPeriodoResult(
                new PeriodoCobro(fechaInicio, ciclo.Fin),
                Redondear(montoBaseCiclo),
                FueProrrateado: false),
        };
    }

    private PrimerPeriodoResult ResolverProrratear(
        DateOnly fechaInicio,
        PeriodoCobro ciclo,
        decimal aporteMes,
        decimal montoBaseCiclo)
    {
        var diasMes = DateTime.DaysInMonth(fechaInicio.Year, fechaInicio.Month);
        var diasRestantes = diasMes - fechaInicio.Day + 1;
        var montoMes0 = aporteMes * diasRestantes / diasMes;
        var montoResto = aporteMes * (_n - 1);
        var monto = Redondear(montoMes0 + montoResto);

        // N=1: solo prorrateo del mes
        if (_n == 1)
            monto = Redondear(aporteMes * diasRestantes / diasMes);

        return new PrimerPeriodoResult(
            new PeriodoCobro(fechaInicio, ciclo.Fin),
            monto,
            FueProrrateado: true);
    }

    private PrimerPeriodoResult ResolverOmitirMesEntrante(
        DateOnly fechaInicio,
        PeriodoCobro ciclo,
        DateOnly ancla,
        decimal aporteMes,
        decimal montoBaseCiclo)
    {
        var mesInicioCiclo = PrimerDiaMes(ciclo.Inicio);
        var mesAlta = PrimerDiaMes(fechaInicio);
        var offsetMes = (mesAlta.Year - mesInicioCiclo.Year) * 12 + (mesAlta.Month - mesInicioCiclo.Month);
        var mesesRestantes = _n - 1 - offsetMes;

        if (mesesRestantes <= 0)
        {
            var siguiente = SiguienteCiclo(ciclo, ancla);
            return new PrimerPeriodoResult(siguiente, Redondear(montoBaseCiclo), FueProrrateado: false);
        }

        var inicioCobro = mesAlta.AddMonths(1);
        var periodo = new PeriodoCobro(inicioCobro, ciclo.Fin);
        var monto = Redondear(aporteMes * mesesRestantes);
        return new PrimerPeriodoResult(periodo, monto, FueProrrateado: false);
    }

    private PeriodoCobro CicloDesdeInicio(DateOnly startDay1)
    {
        var start = PrimerDiaMes(startDay1);
        var finMes = start.AddMonths(_n - 1);
        var fin = new DateOnly(finMes.Year, finMes.Month, DateTime.DaysInMonth(finMes.Year, finMes.Month));
        return new PeriodoCobro(start, fin);
    }

    internal static PoliticaPrimerPeriodo NormalizarPolitica(DateOnly fechaInicio, PoliticaPrimerPeriodo politica)
    {
        if (politica == PoliticaPrimerPeriodo.Prorratear && fechaInicio.Day <= DiaMaximoSinProrrateo)
            return PoliticaPrimerPeriodo.PeriodoCompleto;
        return politica;
    }

    private static DateOnly PrimerDiaMes(DateOnly d) => new(d.Year, d.Month, 1);

    private static int FloorDiv(int a, int b)
    {
        if (b <= 0) throw new ArgumentOutOfRangeException(nameof(b));
        var q = a / b;
        var r = a % b;
        if (r != 0 && a < 0) q--;
        return q;
    }

    private static decimal Redondear(decimal v) =>
        Math.Round(v, 0, MidpointRounding.AwayFromZero);
}
