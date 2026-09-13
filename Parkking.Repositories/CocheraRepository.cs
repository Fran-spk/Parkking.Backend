using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class CocheraRepository
{
    private readonly EstacionamientoContext _context;

    public CocheraRepository(EstacionamientoContext context) => _context = context;

    private IQueryable<Cochera> WithIncludes() =>
        _context.Cocheras
            .Include(c => c.CategoriaCochera)
            .Include(c => c.VehiculosPermitidos)
            .Include(c => c.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Abono);

    public List<Cochera> GetAllActivas(int estacionamientoId) =>
        WithIncludes()
            .Where(c => c.EstacionamientoId == estacionamientoId && c.Activo)
            .OrderBy(c => c.Numero).ToList();

    public Cochera? GetById(int id, int estacionamientoId) =>
        WithIncludes()
            .FirstOrDefault(c => c.CocheraId == id && c.EstacionamientoId == estacionamientoId && c.Activo);

    public List<Cochera> GetActivas(int estacionamientoId) =>
        WithIncludes()
            .Where(c => c.EstacionamientoId == estacionamientoId && c.Activo).ToList();

    public bool ExisteNumero(int estacionamientoId, string numero) =>
        _context.Cocheras.Any(c => c.EstacionamientoId == estacionamientoId && c.Numero == numero && c.Activo);

    public List<TipoVehiculo> GetTiposVehiculo(IEnumerable<int> ids) =>
        _context.TiposVehiculo.Where(v => ids.Contains(v.TipoVehiculoId)).ToList();

    public Cochera? GetForUpdate(int id, int estacionamientoId) =>
        WithIncludes()
            .FirstOrDefault(c => c.CocheraId == id && c.EstacionamientoId == estacionamientoId && c.Activo);

    public Cochera? GetForDeactivate(int id, int estacionamientoId) =>
        _context.Cocheras
            .Include(c => c.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Abono)
            .FirstOrDefault(c => c.CocheraId == id && c.EstacionamientoId == estacionamientoId && c.Activo);

    public void Add(Cochera cochera) => _context.Cocheras.Add(cochera);
    public void SaveChanges() => _context.SaveChanges();
}
