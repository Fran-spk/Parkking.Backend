using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Parkking.Models
{
    public class AbonoCochera: IMultiTenant
    {
        [Key]
        public int AbonoCocheraId { get; set; }

        [ForeignKey("Estacionamiento")]
        public int EstacionamientoId { get; set; }

        [ForeignKey("Cliente")]
        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        [ForeignKey("Cochera")]
        public int CocheraId { get; set; }
        public Cochera Cochera { get; set; }

        [ForeignKey("TipoVehiculo")]
        public int TipoVehiculoId { get; set; }
        public TipoVehiculo TipoVehiculo { get; set; }

        [MaxLength(10)]
        public string? Patente { get; set; }

        [MaxLength(50)]
        public string? ModeloVehiculo { get; set; }

        [MaxLength(100)]
        public string? Cobrador { get; set; }

        [Required]
        public DateOnly FechaInicio { get; set; } // Entrada física del vehículo

        [Required]
        public DateOnly FechaInicioCobro { get; set; } // Primer mes que se va a devengar/cobrar

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecioAcordado { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<PagoMensual> PagosMensuales { get; set; }
            = new List<PagoMensual>();

        public bool TieneDeuda()
        {
            return ObtenerPrimerMesImpago() != null;
        }
        public DateOnly? ObtenerPrimerMesImpago()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var periodo = new DateOnly(
                FechaInicioCobro.Year,
                FechaInicioCobro.Month,
                1);

            var mesActual = new DateOnly(
                hoy.Year,
                hoy.Month,
                1);

            while (periodo <= mesActual)
            {
                bool existePago = PagosMensuales.Any(p =>
                    p.Mes.Year == periodo.Year &&
                    p.Mes.Month == periodo.Month);

                if (!existePago)
                    return periodo;

                periodo = periodo.AddMonths(1);
            }

            return null;
        }
        public int ObtenerDiasAtraso()
        {
            var primerMesImpago = ObtenerPrimerMesImpago();

            if (primerMesImpago == null)
                return 0;

            return (
                DateTime.Today -
                primerMesImpago.Value.ToDateTime(TimeOnly.MinValue)
            ).Days;
        }

        public decimal CalcularMontoMensual(Func<int, int, decimal> obtenerTarifaListaFallback)
        {
            if (PrecioAcordado.HasValue)
            {
                return PrecioAcordado.Value;
            }
            int categoriaId = Cochera?.CategoriaCocheraId ?? 0;

            if (categoriaId == 0)
            {
                throw new InvalidOperationException($"No se puede calcular la tarifa de lista para el abono {AbonoCocheraId} porque la Cochera no está cargada o no tiene categoría asignada.");
            }

            return obtenerTarifaListaFallback(TipoVehiculoId, categoriaId);
        }
    }
}