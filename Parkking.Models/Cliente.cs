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
        public DatosEstacionamiento Estacionamiento { get; set; }
        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }

        /// <summary>DNI / documento del cliente (locatario del contrato).</summary>
        [MaxLength(30)]
        public string? Documento { get; set; }

        /// <summary>Domicilio del cliente para el contrato y notificaciones.</summary>
        [MaxLength(200)]
        public string? Domicilio { get; set; }

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Observacion { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Abono> Abonos { get; set; } = new List<Abono>();
        public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }

}
