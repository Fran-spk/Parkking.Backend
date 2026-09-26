using Parkking.Models.Enums;

namespace Parkking.Models.Cobro;

public static class PeriodicidadStrategyFactory
{
    public static IPeriodicidadStrategy For(PeriodicidadCobro periodicidad) => periodicidad switch
    {
        PeriodicidadCobro.Mensual => new AgrupacionMensualStrategy(1),
        PeriodicidadCobro.Bimestral => new AgrupacionMensualStrategy(2),
        PeriodicidadCobro.Trimestral => new AgrupacionMensualStrategy(3),
        PeriodicidadCobro.Semestral => new AgrupacionMensualStrategy(6),
        PeriodicidadCobro.Quincenal => new QuincenalStrategy(),
        PeriodicidadCobro.Anual => new AnualStrategy(),
        _ => new AgrupacionMensualStrategy(1),
    };

    /// <summary>Ancla = día 1 del mes de fecha de inicio del abono.</summary>
    public static DateOnly AnclaDesdeFechaInicio(DateOnly fechaInicio) =>
        new(fechaInicio.Year, fechaInicio.Month, 1);
}
