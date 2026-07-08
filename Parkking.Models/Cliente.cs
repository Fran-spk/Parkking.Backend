using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models
{

    public class Cliente: IMultiTenant
    {
        [Key]
        public int ClienteId { get; set; }
        [Required]
        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Observacion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<AbonoCochera> Abonos { get; set; }
            = new List<AbonoCochera>();
    }

}
