using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using Parkking_backend.DTOs.Clientes;

namespace Parkking_backend.Services
{
    public class ClienteService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public ClienteService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<Cliente> GetAll(bool includeInactivos = false)
        {
            var query = _context.Clientes
                .Where(c => c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (!includeInactivos)
                query = query.Where(c => c.Activo);

            return query.OrderBy(c => c.Nombre).ToList();
        }

        public Cliente GetById(int id)
        {
            var cliente = _context.Clientes
                .Include(c => c.Abonos.Where(a => a.Activo))
                    .ThenInclude(a => a.Cochera)
                .Include(c => c.Abonos.Where(a => a.Activo))
                    .ThenInclude(a => a.TipoVehiculo)
                .FirstOrDefault(c => c.ClienteId == id);

            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            return cliente;
        }

        public Cliente Create(ClienteRequest request)
        {
            var cliente = new Cliente
            {
                EstacionamientoId = _estacionamiento.EstacionamientoId,
                Nombre = request.Nombre,
                Telefono = request.Telefono,
                Email = request.Email,
                Observacion = request.Observacion,
                Activo = true
            };

            _context.Clientes.Add(cliente);
            _context.SaveChanges();

            return cliente;
        }

        public Cliente Update(int id, ClienteRequest request)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.ClienteId == id);

            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            if (!cliente.Activo)
                throw new Exception("El cliente está dado de baja");

            cliente.Nombre = request.Nombre;
            cliente.Telefono = request.Telefono;
            cliente.Email = request.Email;
            cliente.Observacion = request.Observacion;

            _context.SaveChanges();

            return cliente;
        }

        public void Deactivate(int id)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.ClienteId == id);

            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            if (!cliente.Activo)
                throw new Exception("El cliente ya está dado de baja");

            var tieneAbonosActivos = _context.AbonoCocheras
                .Any(a => a.ClienteId == id && a.Activo);

            if (tieneAbonosActivos)
                throw new Exception("No se puede dar de baja, el cliente tiene abonos activos");

            cliente.Activo = false;
            _context.SaveChanges();
        }

        public void Reactivate(int id)
        {
            var cliente = _context.Clientes
                .FirstOrDefault(c => c.ClienteId == id);

            if (cliente == null)
                throw new Exception("Cliente no encontrado");

            cliente.Activo = true;
            _context.SaveChanges();
        }
    }
}
