using Parkking.Infrastructure.Persistence;
using Parkking.Models.Finanzas;

namespace Parkking.Repositories;

public class TipoGastoRepository
{
    private readonly EstacionamientoContext _context;

    public TipoGastoRepository(EstacionamientoContext context) => _context = context;

    public List<TipoGasto> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var query = _context.TiposGasto.Where(t => t.EstacionamientoId == estacionamientoId);
        if (!includeInactivos) query = query.Where(t => t.Activo);
        return query.OrderBy(t => t.Nombre).ToList();
    }

    public TipoGasto? GetById(int id, int estacionamientoId) =>
        _context.TiposGasto.FirstOrDefault(t =>
            t.TipoGastoId == id && t.EstacionamientoId == estacionamientoId);

    public bool ExisteNombreActivo(string nombre, int estacionamientoId, int? excludeId = null)
    {
        var normalizado = nombre.Trim().ToLower();
        var query = _context.TiposGasto.Where(t =>
            t.EstacionamientoId == estacionamientoId
            && t.Activo
            && t.Nombre.ToLower() == normalizado);
        if (excludeId.HasValue)
            query = query.Where(t => t.TipoGastoId != excludeId);
        return query.Any();
    }

    public void Add(TipoGasto tipo) => _context.TiposGasto.Add(tipo);

    public void SaveChanges() => _context.SaveChanges();
}
