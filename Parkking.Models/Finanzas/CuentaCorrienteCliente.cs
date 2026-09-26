using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models.Finanzas;

/// <summary>Ledger del cliente. Una por cliente. El saldo es la suma de sus movimientos.</summary>
public class CuentaCorrienteCliente : IMultiTenant
{
    [Key]
    public int CuentaCorrienteClienteId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Cliente))]
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Saldo { get; set; }

    public static CuentaCorrienteCliente Abrir(int estacionamientoId, int clienteId) => new()
    {
        EstacionamientoId = estacionamientoId,
        ClienteId = clienteId,
        Saldo = 0
    };

    /// <summary>
    /// El importe del movimiento es el del estacionamiento y del grupo.
    /// Cobro y gasto se invierten en el cliente. El reintegro y el ajuste se aplican con el mismo signo:
    /// un ajuste a favor del cliente es negativo en el grupo y baja lo que el cliente debe.
    /// </summary>
    public void Aplicar(Movimiento movimiento) =>
        Saldo += movimiento.Tipo is TipoMovimiento.Reintegro or TipoMovimiento.Ajuste
            ? movimiento.Importe
            : -movimiento.Importe;
}
