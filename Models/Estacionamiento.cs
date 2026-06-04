namespace Parkking_backend.Models
{
    public class Estacionamiento
    {
        public int EstacionamientoId { get; set; }
        public string Nombre { get; set; } // direccion tambien puede ser 
        public int DiaVencimientoAbono { get; set; }   // ej: 10
        public bool AplicaRecargo { get; set; }         // si cobra recargo o no
        public decimal PorcentajeRecargo { get; set; }  // ej: 10 = 10%
        public int? DiasUmbralProporcional { get; set; }
    }
}
