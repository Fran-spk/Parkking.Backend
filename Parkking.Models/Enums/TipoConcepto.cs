using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
namespace Parkking.Models.Enums
{

    public enum TipoConcepto
    {
        [Description("Pago mensual")]
        PagoMensual,

        [Description("Reintegro cliente")]
        ReintegroCliente,

        [Description("Cargo cliente")]
        CargoCliente,

        [Description("Gasto cochera")]
        GastoCochera

    }
}
