using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using Parkking_backend.DTOs.Cocheras;

namespace Parkking_backend.Services
{
    public class CocheraService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public CocheraService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<Cochera> GetAll()
        {
            return _context.Cocheras
                .Include(c => c.CategoriaCochera)
                .Include(c => c.VehiculosPermitidos)
                .Include(c => c.Abonos)
                .Where(c => c.EstacionamientoId == _estacionamiento.EstacionamientoId && c.Activo)
                .OrderBy(c => c.Numero)
                .ToList();
        }

        public Cochera GetById(int id)
        {
            return _context.Cocheras
                .Include(c => c.CategoriaCochera)
                .Include(c => c.VehiculosPermitidos)
                .Include(c => c.Abonos)
                .FirstOrDefault(c => c.CocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                                   && c.Activo);
        }

        public List<Cochera> GetLibresByTipoVehiculo(int tipoVehiculoId)
        {
            var cocheras = _context.Cocheras
                .Include(c => c.CategoriaCochera)
                .Include(c => c.VehiculosPermitidos)
                .Include(c => c.Abonos)
                .Where(c => c.EstacionamientoId == _estacionamiento.EstacionamientoId
                         && c.Activo)
                .ToList();

            var disponibles = cocheras
                .Where(c =>
                    c.EstadoCochera == EstadoCochera.Habilitada &&
                    c.VehiculosPermitidos.Any(v => v.TipoVehiculoId == tipoVehiculoId) &&
                    c.EstaDisponible()
                )
                .OrderBy(c => c.Numero)
                .ToList();

            return disponibles;
        }

        public Cochera Create(CocheraRequest request)
        {
            var existe = _context.Cocheras
                .Any(c => c.EstacionamientoId == _estacionamiento.EstacionamientoId
                       && c.Numero == request.Numero
                       && c.Activo);

            if (existe)
                throw new Exception($"Ya existe una cochera con el número {request.Numero}");

            if (request.EstadoCochera == EstadoCochera.Habilitada && !string.IsNullOrEmpty(request.Observacion))
                throw new Exception("No se puede cargar observación si la cochera está habilitada");

            var cochera = new Cochera
            {
                EstacionamientoId = _estacionamiento.EstacionamientoId,
                Numero = request.Numero,
                Observacion = request.Observacion,
                EstadoCochera = request.EstadoCochera,
                CategoriaCocheraId = request.CategoriaCocheraId,
                MultipleOcupacion = request.MultipleOcupacion,
                Activo = true
            };

            _context.Cocheras.Add(cochera);
            if (request.VehiculosPermitidosIds != null && request.VehiculosPermitidosIds.Any())
            {
                var vehiculos = _context.TiposVehiculo
                    .Where(v => request.VehiculosPermitidosIds.Contains(v.TipoVehiculoId))
                    .ToList();

                cochera.VehiculosPermitidos = vehiculos;
            }
            _context.SaveChanges();

            return cochera;
        }

        public Cochera Update(int id, CocheraRequest request)
        {
            var cochera = _context.Cocheras
                .Include(c => c.CategoriaCochera)
                .Include(c => c.Abonos)
                .Include(c => c.VehiculosPermitidos)
                .FirstOrDefault(c => c.CocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                                   && c.Activo);

            if (cochera == null)
                throw new Exception("Cochera no encontrada");

            if (request.EstadoCochera == EstadoCochera.Habilitada && !string.IsNullOrEmpty(request.Observacion))
                throw new Exception("No se puede cargar observación si la cochera está habilitada");

            cochera.Numero = request.Numero;
            cochera.Observacion = request.Observacion;
            cochera.EstadoCochera = request.EstadoCochera;
            cochera.CategoriaCocheraId = request.CategoriaCocheraId;
            cochera.MultipleOcupacion = request.MultipleOcupacion;

            cochera.VehiculosPermitidos.Clear();

            if (request.VehiculosPermitidosIds != null)
            {
                var vehiculos = _context.TiposVehiculo
                    .Where(v => request.VehiculosPermitidosIds.Contains(v.TipoVehiculoId))
                    .ToList();

                foreach (var v in vehiculos)
                {
                    cochera.VehiculosPermitidos.Add(v);
                }
            }

            _context.SaveChanges();

            return cochera;
        }

        public void Deactivate(int id)
        {
            var cochera = _context.Cocheras
                .Include(c => c.Abonos)
                .FirstOrDefault(c => c.CocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                                   && c.Activo);

            if (cochera == null)
                throw new Exception("Cochera no encontrada");

            if (cochera.Abonos.Any(a => a.Activo))
                throw new Exception("No se puede desactivar una cochera con abonos activos");

            cochera.Activo = false;
            _context.SaveChanges();
        }
    }
}
