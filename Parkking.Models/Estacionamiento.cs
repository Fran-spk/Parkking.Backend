using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models
{
    public class Estacionamiento
    {
        public int EstacionamientoId { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public int DiaVencimientoAbono { get; set; }   
        public bool AplicaRecargo { get; set; }        
        public decimal PorcentajeRecargo { get; set; } 
        public int? DiasUmbralProporcional { get; set; }

        [NotMapped]
        public bool activo { get; set; }
    }
}
