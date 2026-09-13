namespace Parkking.Models.Enums;

/// <summary>Origen de una línea de liquidación de cuota.</summary>
public enum TipoDetalleCuota
{
    TarifaLista = 0,
    PrecioAcordado = 1,
    Prorrateo = 2,
    Recargo = 3,
    Descuento = 4,
    AjusteManual = 5
}
