using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models.Finanzas;

/// <summary>Ledger del estacionamiento. Una por tenant.</summary>
public class CuentaCorrienteEstacionamiento : IMultiTenant
{
    [Key]
    public int CuentaCorrienteEstacionamientoId { get; set; }

    public int EstacionamientoId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Saldo { get; set; }

    public static CuentaCorrienteEstacionamiento Abrir(int estacionamientoId) => new()
    {
        EstacionamientoId = estacionamientoId,
        Saldo = 0
    };

    public void Aplicar(Movimiento movimiento) => Saldo += movimiento.Importe;
}
