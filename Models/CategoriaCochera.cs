namespace Parkking_backend.Models
{
    public class CategoriaCochera
    {
        public int CategoriaCocheraId { get; set; }
        public int EstacionamientoId { get; set; }
        public Estacionamiento Estacionamiento { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; } = true;
    }
}
