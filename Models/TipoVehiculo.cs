using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parkking_backend.Models;

namespace MODELO
{
    public class TipoVehiculo
    {
        [Key]
        public int TipoVehiculoId { get; set; }
        [Required]
        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }

        public List<Cochera> Cocheras { get; set; } = new List<Cochera>();
    }
}
