namespace Parkking.Models.Cobro;

/// <summary>Rango de cobro inclusivo [Inicio, Fin].</summary>
public readonly record struct PeriodoCobro(DateOnly Inicio, DateOnly Fin)
{
    public int Dias => Fin.DayNumber - Inicio.DayNumber + 1;

    public bool Contiene(DateOnly fecha) => fecha >= Inicio && fecha <= Fin;

    public override string ToString() => $"{Inicio:yyyy-MM-dd} → {Fin:yyyy-MM-dd}";
}
