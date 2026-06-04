namespace Parkking_backend.DTOs.Caja
{
    public class MovimientoCajaDto
    {
        public int MovimientoCajaId { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal Monto { get; set; }

        public int Tipo { get; set; }
        public string TipoDescripcion { get; set; }

        public int TipoConcepto { get; set; }
        public string TipoConceptoDescripcion { get; set; }

        public string Descripcion { get; set; }
        public string? Responsable { get; set; }
    }
}
