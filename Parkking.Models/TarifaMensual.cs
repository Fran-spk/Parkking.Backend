using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Parkking.Models
{
    public class TarifaMensual: IMultiTenant
    {
        [Key]
        public int TarifaMensualId { get; set; }

        [Required]
        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }

        [ForeignKey("TipoVehiculo")]
        public int TipoVehiculoId { get; set; }
        public TipoVehiculo TipoVehiculo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        [Required]
        public DateTime FechaHoraActualizacion { get; set; }

        public int CategoriaCocheraId { get; set; }
        public CategoriaCochera CategoriaCochera { get; set; }
    }
}
