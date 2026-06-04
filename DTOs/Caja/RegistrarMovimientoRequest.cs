using MODELO;
using Parkking_backend.Models;

namespace Parkking_backend.DTOs.Caja
{
    public class RegistrarMovimientoRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public TipoConcepto TipoConcepto { get; set; }
        public string Descripcion { get; set; }
        public string? Responsable { get; set; }
        public decimal Monto { get; set; }
        public string? ClienteNombre { get; set; }
        public int? AbonoCocheraId { get; set; }
    }

    
}
