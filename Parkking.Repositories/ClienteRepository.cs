using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class ClienteRepository
{
    private readonly EstacionamientoContext _context;

    public ClienteRepository(EstacionamientoContext context) => _context = context;

    public List<Cliente> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var clientes = _context.Clientes.ToList();
        if (!includeInactivos)
            clientes = clientes.Where(c => c.Activo).ToList();
        return clientes.OrderBy(c => c.Nombre).ToList();
    }

    public Cliente? GetByIdWithAbonos(int id)
    {
        return _context.Clientes
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.Plazas).ThenInclude(p => p.Cochera)
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .FirstOrDefault(c => c.ClienteId == id);
    }

    public Cliente? GetById(int id) => _context.Clientes.FirstOrDefault(c => c.ClienteId == id);

    public List<Vehiculo> GetVehiculosByCliente(int clienteId) =>
        _context.Vehiculos
            .Include(v => v.TipoVehiculo)
            .Include(v => v.AbonoVehiculo).ThenInclude(av => av!.Abono)
            .Where(v => v.ClienteId == clienteId && v.Activo)
            .OrderBy(v => v.Patente)
            .ToList();

    public void Add(Cliente cliente) => _context.Clientes.Add(cliente);

    public bool TieneAbonosActivos(int clienteId) =>
        _context.Abonos.Any(a => a.ClienteId == clienteId && a.Activo);

    public void SaveChanges() => _context.SaveChanges();
}
