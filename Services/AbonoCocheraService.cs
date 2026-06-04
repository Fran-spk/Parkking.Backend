using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.DTOs.Abonos;

namespace Parkking_backend.Services
{
    public class AbonoCocheraService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public AbonoCocheraService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<AbonoCochera> GetAll()
        {
            var abonos = AbonoCocheraMapper.WithIncludes(_context.AbonoCocheras)
                .Where(a => a.Cochera.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderBy(a => a.Cochera.Numero)
                .ToList();

            return abonos;
        }

        public List<AbonoCochera> GetActivos()
        {
            var abonos = AbonoCocheraMapper.WithIncludes(_context.AbonoCocheras)
                .Where(a => a.Activo &&
                            a.Cochera.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderBy(a => a.Cochera.Numero)
                .ToList();

            return abonos;
        }

        public List<AbonoCochera> GetByCliente(int clienteId)
        {
            var abonos = AbonoCocheraMapper.WithIncludes(_context.AbonoCocheras)
                .Where(a => a.ClienteId == clienteId &&
                            a.Cochera.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderBy(a => a.Cochera.Numero)
                .ToList();

            return abonos;
        }

        public List<AbonoCochera> GetByCochera(int cocheraId)
        {
            var abonos = AbonoCocheraMapper.WithIncludes(_context.AbonoCocheras)
                .Where(a => a.CocheraId == cocheraId)
                .OrderByDescending(a => a.FechaInicio)
                .ToList();

            return abonos;
        }

        public AbonoCochera GetByPatente(string patente)
        {
            var abono = AbonoCocheraMapper.WithIncludes(_context.AbonoCocheras)
                .Where(a => a.Patente == patente &&
                            a.Cochera.EstacionamientoId == _estacionamiento.EstacionamientoId)
                .OrderByDescending(a => a.Activo)
                .ThenByDescending(a => a.FechaInicio)
                .FirstOrDefault();

            if (abono == null)
                throw new Exception("No se encontró ningún abono con esa patente");

            return abono;
        }

        public AbonoCochera Create(CrearAbonoRequest request)
        {
            // 1. Validaciones básicas de existencia
            var cliente = _context.Clientes.Find(request.ClienteId);
            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            var cochera = _context.Cocheras.Find(request.CocheraId);
            if (cochera == null)
                throw new Exception("Cochera no encontrada");

            // 2. Parseo y normalización de las fechas que vienen del Front (yyyy-MM-dd)
            if (!DateOnly.TryParseExact(request.FechaInicio, "yyyy-MM-dd", out DateOnly fechaInicioParsed))
            {
                throw new Exception("El formato de FechaInicio debe ser yyyy-MM-dd");
            }

            DateOnly fechaInicioCobroParsed = fechaInicioParsed;
            if (!string.IsNullOrEmpty(request.FechaInicioCobro))
            {
                if (!DateOnly.TryParseExact(request.FechaInicioCobro, "yyyy-MM-dd", out DateOnly parsedCobro))
                {
                    throw new Exception("El formato de FechaInicioCobro debe ser yyyy-MM-dd");
                }
                fechaInicioCobroParsed = parsedCobro;
            }

            // 3. Mapeo a la entidad del modelo de la Base de Datos
            var nuevoAbono = new AbonoCochera
            {
                ClienteId = request.ClienteId,
                CocheraId = request.CocheraId,
                TipoVehiculoId = request.TipoVehiculoId,
                Patente = request.Patente,
                ModeloVehiculo = request.ModeloVehiculo,
                Cobrador = request.Cobrador,
                PrecioAcordado = request.PrecioAcordado,
                Activo = true,
                FechaInicio = fechaInicioParsed,
                FechaInicioCobro = fechaInicioCobroParsed
            };

            _context.AbonoCocheras.Add(nuevoAbono);
            _context.SaveChanges();

            return nuevoAbono;
        }

        public AbonoCochera Update(int id, ModificarAbonoRequest request)
        {
            var abono = _context.AbonoCocheras
                .FirstOrDefault(a => a.AbonoCocheraId == id && a.Activo);

            if (abono == null)
                throw new Exception("Abono no encontrado");

            abono.Patente = request.Patente;
            abono.ModeloVehiculo = request.ModeloVehiculo;
            abono.Cobrador = request.Cobrador;
            abono.PrecioAcordado = request.PrecioAcordado;

            _context.SaveChanges();

            var actualizado = AbonoCocheraMapper.LoadWithIncludes(_context, id);
            return actualizado;
        }

        public void MoveCochera(int id, int nuevaCocheraId)
        {
            var abono = _context.AbonoCocheras
                .Include(a => a.Cochera)
                .FirstOrDefault(a => a.AbonoCocheraId == id && a.Activo);

            if (abono == null)
                throw new Exception("Abono no encontrado");

            var nuevaCochera = _context.Cocheras
                .Include(c => c.Abonos)
                .Include(c => c.VehiculosPermitidos)
                .FirstOrDefault(c => c.CocheraId == nuevaCocheraId
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                                   && c.Activo);

            if (nuevaCochera == null)
                throw new Exception("Cochera destino no encontrada");

            if (!nuevaCochera.VehiculosPermitidos.Any(v => v.TipoVehiculoId == abono.TipoVehiculoId))
                throw new Exception("El tipo de vehículo no está permitido en la cochera destino");

            var activos = nuevaCochera.Abonos.Count(a => a.Activo);

            if (activos > 0 && !nuevaCochera.MultipleOcupacion)
                throw new Exception("La cochera destino no admite múltiples ocupaciones");

            abono.CocheraId = nuevaCocheraId;

            _context.SaveChanges();
        }

        public void SwapCocheras(SwapCocherasRequest request)
        {
            var abono1 = _context.AbonoCocheras
                .FirstOrDefault(a => a.AbonoCocheraId == request.AbonoCocheraId1 && a.Activo);

            var abono2 = _context.AbonoCocheras
                .FirstOrDefault(a => a.AbonoCocheraId == request.AbonoCocheraId2 && a.Activo);

            if (abono1 == null || abono2 == null)
                throw new Exception("Uno o ambos abonos no encontrados");

            var temp = abono1.CocheraId;
            abono1.CocheraId = abono2.CocheraId;
            abono2.CocheraId = temp;

            _context.SaveChanges();
        }

        public void Deactivate(int id)
        {
            var abono = _context.AbonoCocheras
                .FirstOrDefault(a => a.AbonoCocheraId == id && a.Activo);

            if (abono == null)
                throw new Exception("Abono no encontrado");

            abono.Activo = false;

            _context.SaveChanges();
        }
    }
}
