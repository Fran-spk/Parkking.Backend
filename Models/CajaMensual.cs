using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Parkking_backend.Models
{
    public class CajaMensual
    {
        [Key]
        public int CajaMensualId { get; set; }

        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }

        [Required]
        public DateOnly Mes { get; set; }

        public bool Cerrada { get; set; } = false;

        public ICollection<MovimientoCaja> Movimientos { get; set; }
            = new List<MovimientoCaja>();

        [NotMapped]
        public decimal TotalIngresos => Movimientos
            .Where(m => m.Tipo == TipoMovimiento.Ingreso)
            .Sum(m => m.Monto);

        [NotMapped]
        public decimal TotalGastos => Movimientos
            .Where(m => m.Tipo == TipoMovimiento.Egreso)
            .Sum(m => m.Monto);

        [NotMapped]
        public decimal Saldo => TotalIngresos - TotalGastos;
    }
}
