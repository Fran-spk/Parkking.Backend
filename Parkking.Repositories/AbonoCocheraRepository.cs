using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class AbonoCocheraRepository
{
    private readonly EstacionamientoContext _context;

    public AbonoCocheraRepository(EstacionamientoContext context) => _context = context;

    public IQueryable<AbonoCochera> WithIncludes(IQueryable<AbonoCochera> query) =>
        query.Include(a => a.Cliente)
             .Include(a => a.Cochera).ThenInclude(c => c.CategoriaCochera)
             .Include(a => a.TipoVehiculo);

    public List<AbonoCochera> GetAll(int estacionamientoId) =>
        WithIncludes(_context.AbonoCocheras)
            .Where(a => a.Cochera.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.Cochera.Numero).ToList();

    public List<AbonoCochera> GetActivos(int estacionamientoId) =>
        WithIncludes(_context.AbonoCocheras)
            .Where(a => a.Activo && a.Cochera.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.Cochera.Numero).ToList();

    public List<AbonoCochera> GetByCliente(int clienteId, int estacionamientoId) =>
        WithIncludes(_context.AbonoCocheras)
            .Where(a => a.ClienteId == clienteId && a.Cochera.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.Cochera.Numero).ToList();

    public List<AbonoCochera> GetByCochera(int cocheraId) =>
        WithIncludes(_context.AbonoCocheras)
            .Where(a => a.CocheraId == cocheraId)
            .OrderByDescending(a => a.FechaInicio).ToList();

    public AbonoCochera? GetByPatente(string patente, int estacionamientoId) =>
        WithIncludes(_context.AbonoCocheras)
            .Where(a => a.Patente == patente && a.Cochera.EstacionamientoId == estacionamientoId)
            .OrderByDescending(a => a.Activo).ThenByDescending(a => a.FechaInicio)
            .FirstOrDefault();

    public AbonoCochera? GetActivoById(int id) =>
        _context.AbonoCocheras.FirstOrDefault(a => a.AbonoCocheraId == id && a.Activo);

    public AbonoCochera? GetActivoWithCochera(int id) =>
        _context.AbonoCocheras.Include(a => a.Cochera)
            .FirstOrDefault(a => a.AbonoCocheraId == id && a.Activo);

    public AbonoCochera? LoadWithIncludes(int id) =>
        WithIncludes(_context.AbonoCocheras).FirstOrDefault(a => a.AbonoCocheraId == id);

    public Cliente? GetCliente(int id) => _context.Clientes.Find(id);
    public Cochera? GetCochera(int id) => _context.Cocheras.Find(id);

    public Cochera? GetCocheraDestino(int cocheraId, int estacionamientoId) =>
        _context.Cocheras.Include(c => c.Abonos).Include(c => c.VehiculosPermitidos)
            .FirstOrDefault(c => c.CocheraId == cocheraId && c.EstacionamientoId == estacionamientoId && c.Activo);

    public void Add(AbonoCochera abono) => _context.AbonoCocheras.Add(abono);
    public void SaveChanges() => _context.SaveChanges();
}
