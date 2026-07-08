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
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.Cochera)
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.TipoVehiculo)
            .FirstOrDefault(c => c.ClienteId == id);
    }

    public Cliente? GetById(int id) => _context.Clientes.FirstOrDefault(c => c.ClienteId == id);

    public void Add(Cliente cliente) => _context.Clientes.Add(cliente);

    public bool TieneAbonosActivos(int clienteId) =>
        _context.AbonoCocheras.Any(a => a.ClienteId == clienteId && a.Activo);

    public void SaveChanges() => _context.SaveChanges();
}
