using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;

namespace Parkking_backend.Services
{
    public class TipoVehiculoService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public TipoVehiculoService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<TipoVehiculo> GetAll(bool includeInactivos = false)
        {
            var query = _context.TiposVehiculo
                .Where(t => t.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (!includeInactivos)
                query = query.Where(t => t.Activo);

            return query.OrderBy(t => t.Nombre).ToList();
        }

        public TipoVehiculo GetById(int id)
        {
            return _context.TiposVehiculo
                .FirstOrDefault(t => t.TipoVehiculoId == id
                                   && t.EstacionamientoId == _estacionamiento.EstacionamientoId);
        }

        public TipoVehiculo Create(string nombre)
        {
            var existe = _context.TiposVehiculo
                .Any(t => t.Nombre == nombre
                       && t.EstacionamientoId == _estacionamiento.EstacionamientoId
                       && t.Activo);

            if (existe)
                throw new Exception("Ya existe un tipo de vehículo con ese nombre");

            var tipo = new TipoVehiculo
            {
                EstacionamientoId = _estacionamiento.EstacionamientoId,
                Nombre = nombre,
                Activo = true
            };

            _context.TiposVehiculo.Add(tipo);
            _context.SaveChanges();
            return tipo;
        }

        public TipoVehiculo Update(int id, string nuevoNombre)
        {
            var tipo = _context.TiposVehiculo
                .FirstOrDefault(t => t.TipoVehiculoId == id
                                   && t.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (tipo == null)
                throw new Exception("Tipo de vehículo no encontrado");

            if (!tipo.Activo)
                throw new Exception("El tipo de vehículo está dado de baja");

            var nombreDuplicado = _context.TiposVehiculo
                .Any(t => t.Nombre == nuevoNombre
                       && t.TipoVehiculoId != id
                       && t.EstacionamientoId == _estacionamiento.EstacionamientoId
                       && t.Activo);

            if (nombreDuplicado)
                throw new Exception("Ya existe un tipo de vehículo con ese nombre");

            tipo.Nombre = nuevoNombre;
            _context.SaveChanges();
            return tipo;
        }

        public void Deactivate(int id)
        {
            var tipo = _context.TiposVehiculo
                .FirstOrDefault(t => t.TipoVehiculoId == id
                                   && t.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (tipo == null)
                throw new Exception("Tipo de vehículo no encontrado");

            if (!tipo.Activo)
                throw new Exception("El tipo de vehículo ya está dado de baja");

            var tieneAbonosActivos = _context.AbonoCocheras
                .Any(a => a.TipoVehiculoId == id && a.Activo);

            if (tieneAbonosActivos)
                throw new Exception("No se puede dar de baja, existen abonos activos con este tipo de vehículo");

            tipo.Activo = false;
            _context.SaveChanges();
        }

        public void Reactivate(int id)
        {
            var tipo = _context.TiposVehiculo
                .FirstOrDefault(t => t.TipoVehiculoId == id
                                   && t.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (tipo == null)
                throw new Exception("Tipo de vehículo no encontrado");

            tipo.Activo = true;
            _context.SaveChanges();
        }
    }
}
