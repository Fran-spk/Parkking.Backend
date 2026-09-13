using System.ComponentModel;

namespace Parkking.Models.Enums;

public enum TipoConcepto
{
    [Description("Pago abono")]
    PagoAbono = 0,

    [Description("Reintegro cliente")]
    ReintegroCliente = 1,

    [Description("Cargo cliente")]
    CargoCliente = 2,

    [Description("Gasto cochera")]
    GastoCochera = 3
}
