using Parkking_backend.DTOs.Shared;

namespace Parkking_backend.DTOs.Cocheras
{
    public class CocheraRequest
    {
        public string Numero { get; set; }
        public int CategoriaCocheraId { get; set; }

        public EstadoCochera EstadoCochera { get; set; }

        public string? Observacion { get; set; }

        public bool MultipleOcupacion { get; set; }

        public List<int>? VehiculosPermitidosIds { get; set; }
    }

    public class CocheraResponseDto
    {
        public int CocheraId { get; set; }
        public string Numero { get; set; }

        public int CategoriaCocheraId { get; set; }

        public EstadoCochera EstadoCochera { get; set; }

        public string? Observacion { get; set; }

        public bool MultipleOcupacion { get; set; }

        public bool EstaDisponible { get; set; }

        public List<int> VehiculosPermitidosIds { get; set; }

        public CategoriaCocheraResumenDto? CategoriaCochera { get; set; }
    }
}
