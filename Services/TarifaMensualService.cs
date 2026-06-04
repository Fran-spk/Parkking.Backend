using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using Parkking_backend.DTOs.Tarifas;
using Modelo_Ids;

namespace Parkking_backend.Services
{
    public class TarifaMensualService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public TarifaMensualService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<TarifaVigenteDto> GetVigentes()
        {
            var tarifas = _context.TarifasMensuales
                .Where(t => t.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .GroupBy(t => new { t.TipoVehiculoId, t.CategoriaCocheraId })
                .Select(g => g
                    .OrderByDescending(t => t.FechaHoraActualizacion)
                    .Select(t => new TarifaVigenteDto
                    {
                        TarifaMensualId = t.TarifaMensualId,
                        TipoVehiculoId = t.TipoVehiculoId,
                        CategoriaCocheraId = t.CategoriaCocheraId,
                        Precio = t.Precio,
                        FechaActualizacion = t.FechaHoraActualizacion
                    })
                    .First()
                )
                .ToList();

            return tarifas;
        }

        public TarifaVigenteDto GetVigente(int tipoVehiculoId, int categoriaCocheraId)
        {
            var tarifa = _context.TarifasMensuales
                .Where(t => t.TipoVehiculoId == tipoVehiculoId
                         && t.CategoriaCocheraId == categoriaCocheraId
                         && t.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderByDescending(t => t.FechaHoraActualizacion)
                .Select(t => new TarifaVigenteDto
                {
                    TarifaMensualId = t.TarifaMensualId,
                    TipoVehiculoId = t.TipoVehiculoId,
                    CategoriaCocheraId = t.CategoriaCocheraId,
                    Precio = t.Precio,
                    FechaActualizacion = t.FechaHoraActualizacion
                })
                .FirstOrDefault();

            if (tarifa == null)
                throw new Exception("No hay tarifa vigente para esta combinación");

            return tarifa;
        }

        public List<TarifaHistorialDto> GetHistorial(int tipoVehiculoId, int categoriaCocheraId)
        {
            var historial = _context.TarifasMensuales
                .Where(t => t.TipoVehiculoId == tipoVehiculoId
                         && t.CategoriaCocheraId == categoriaCocheraId
                         && t.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderByDescending(t => t.FechaHoraActualizacion)
                .Select(t => new TarifaHistorialDto
                {
                    TarifaMensualId = t.TarifaMensualId,
                    Precio = t.Precio,
                    FechaActualizacion = t.FechaHoraActualizacion
                })
                .ToList();

            return historial;
        }

        public List<TarifaMensual> GetAll()
        {
            return _context.TarifasMensuales
                .Where(t => t.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderByDescending(t => t.FechaHoraActualizacion)
                .ToList();
        }

        public TarifaMensual Create(CrearTarifaRequest request)
        {
            var tarifa = new TarifaMensual
            {
                EstacionamientoId = _estacionamiento.EstacionamientoId,
                TipoVehiculoId = request.TipoVehiculoId,
                CategoriaCocheraId = request.CategoriaCocheraId,
                Precio = request.Precio,
                FechaHoraActualizacion = DateTime.UtcNow
            };

            _context.TarifasMensuales.Add(tarifa);
            _context.SaveChanges();
            return tarifa;
        }
    }
}

namespace Parkking_backend.Services
{
    public class CrearTarifaRequest
    {
        public int TipoVehiculoId { get; set; }
        public int CategoriaCocheraId { get; set; }
        public decimal Precio { get; set; }
    }
}
