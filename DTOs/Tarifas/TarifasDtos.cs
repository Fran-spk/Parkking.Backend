namespace Parkking_backend.DTOs.Tarifas
{
    public class AgregarTarifaRequest
    {
        public int TipoVehiculoId { get; set; }
        public int CategoriaCocheraId { get; set; }
        public decimal Precio { get; set; }
    }
    public class TarifaVigenteDto
    {
        public int TarifaMensualId { get; set; }
        public int TipoVehiculoId { get; set; }
        public int CategoriaCocheraId { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
    public class TarifaHistorialDto
    {
        public int TarifaMensualId { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
