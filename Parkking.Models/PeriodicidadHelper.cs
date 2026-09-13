using Parkking.Models.Enums;

namespace Parkking.Models;

/// <summary>
/// Cálculo de períodos de cobro según periodicidad.
/// Quincenal: 1–15 y 16–fin de mes.
/// </summary>
public static class PeriodicidadHelper
{
    public static (DateOnly Inicio, DateOnly Fin) PeriodoQueContiene(DateOnly fecha, PeriodicidadCobro periodicidad)
    {
        return periodicidad switch
        {
            PeriodicidadCobro.Quincenal => fecha.Day <= 15
                ? (new DateOnly(fecha.Year, fecha.Month, 1), new DateOnly(fecha.Year, fecha.Month, 15))
                : (new DateOnly(fecha.Year, fecha.Month, 16),
                   new DateOnly(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month))),

            PeriodicidadCobro.Bimestral => PeriodoMensualAgrupado(fecha, 2),
            PeriodicidadCobro.Trimestral => PeriodoMensualAgrupado(fecha, 3),
            PeriodicidadCobro.Semestral => PeriodoMensualAgrupado(fecha, 6),
            PeriodicidadCobro.Anual => (
                new DateOnly(fecha.Year, 1, 1),
                new DateOnly(fecha.Year, 12, 31)),

            _ => (
                new DateOnly(fecha.Year, fecha.Month, 1),
                new DateOnly(fecha.Year, fecha.Month, DateTime.DaysInMonth(fecha.Year, fecha.Month)))
        };
    }

    /// <summary>
    /// Períodos desde el que contiene FechaInicioCobro hasta el que contiene 'hasta' (inclusive).
    /// </summary>
    public static IEnumerable<(DateOnly Inicio, DateOnly Fin)> EnumerarPeriodos(
        DateOnly fechaInicioCobro,
        DateOnly hasta,
        PeriodicidadCobro periodicidad)
    {
        var (inicio, fin) = PeriodoQueContiene(fechaInicioCobro, periodicidad);
        var limite = PeriodoQueContiene(hasta, periodicidad).Inicio;

        while (inicio <= limite)
        {
            yield return (inicio, fin);
            (inicio, fin) = SiguientePeriodo(inicio, fin, periodicidad);
        }
    }

    public static (DateOnly Inicio, DateOnly Fin) SiguientePeriodo(
        DateOnly inicioActual,
        DateOnly finActual,
        PeriodicidadCobro periodicidad)
    {
        var siguiente = finActual.AddDays(1);
        return PeriodoQueContiene(siguiente, periodicidad);
    }

    private static (DateOnly Inicio, DateOnly Fin) PeriodoMensualAgrupado(DateOnly fecha, int meses)
    {
        var mesInicioGrupo = ((fecha.Month - 1) / meses) * meses + 1;
        var inicio = new DateOnly(fecha.Year, mesInicioGrupo, 1);
        var finMes = inicio.AddMonths(meses - 1);
        var fin = new DateOnly(finMes.Year, finMes.Month, DateTime.DaysInMonth(finMes.Year, finMes.Month));
        return (inicio, fin);
    }
}
