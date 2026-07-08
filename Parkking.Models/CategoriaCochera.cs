using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models
{
    public class CategoriaCochera: IMultiTenant
    {
        [Key]
        public int CategoriaCocheraId { get; set; }

        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; } = true;
    }
}
