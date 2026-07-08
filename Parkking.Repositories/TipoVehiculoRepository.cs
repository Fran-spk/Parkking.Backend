using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class TipoVehiculoRepository
{
    private readonly EstacionamientoContext _context;

    public TipoVehiculoRepository(EstacionamientoContext context) => _context = context;

    public List<TipoVehiculo> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var query = _context.TiposVehiculo.Where(t => t.EstacionamientoId == estacionamientoId);
        if (!includeInactivos) query = query.Where(t => t.Activo);
        return query.OrderBy(t => t.Nombre).ToList();
    }

    public TipoVehiculo? GetById(int id, int estacionamientoId) =>
        _context.TiposVehiculo.FirstOrDefault(t => t.TipoVehiculoId == id && t.EstacionamientoId == estacionamientoId);

    public bool ExisteNombre(string nombre, int estacionamientoId, int? excludeId = null)
    {
        var query = _context.TiposVehiculo.Where(t =>
            t.Nombre == nombre && t.EstacionamientoId == estacionamientoId && t.Activo);
        if (excludeId.HasValue) query = query.Where(t => t.TipoVehiculoId != excludeId);
        return query.Any();
    }

    public bool TieneAbonosActivos(int tipoVehiculoId) =>
        _context.AbonoCocheras.Any(a => a.TipoVehiculoId == tipoVehiculoId && a.Activo);

    public void Add(TipoVehiculo tipo) => _context.TiposVehiculo.Add(tipo);
    public void SaveChanges() => _context.SaveChanges();
}
