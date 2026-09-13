using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Parkking.Models.Enums;

namespace Parkking.Models
{
    public class CajaMensual: IMultiTenant
    {
        [Key]
        public int CajaMensualId { get; set; }

        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public DatosEstacionamiento Estacionamiento { get; set; }

        [Required]
        public DateOnly Mes { get; set; }

        public bool Cerrada { get; set; } = false;

        public ICollection<MovimientoCaja> Movimientos { get; set; }
            = new List<MovimientoCaja>();

        public decimal TotalIngresos { get; private set; }
        public decimal TotalGastos { get; private set; }
        public decimal Saldo { get; private set; }

        // Método de negocio dentro de la entidad
        public void RegistrarImpactoFinanciero(TipoMovimiento tipo, decimal monto)
        {
            if (tipo == TipoMovimiento.Ingreso)
                TotalIngresos += monto;
            else
                TotalGastos += monto;

            Saldo = TotalIngresos - TotalGastos;
        }


    }
}
