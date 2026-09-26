using Parkking.Infrastructure.Persistence;
using Parkking.Models.Finanzas;

namespace Parkking.Repositories;

public class GrupoFinancieroRepository
{
    private readonly EstacionamientoContext _context;

    public GrupoFinancieroRepository(EstacionamientoContext context) => _context = context;

    public List<GrupoFinanciero> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var query = _context.GruposFinancieros.Where(g => g.EstacionamientoId == estacionamientoId);
        if (!includeInactivos) query = query.Where(g => g.Activo);
        return query.OrderBy(g => g.Nombre).ToList();
    }

    public GrupoFinanciero? GetById(int id, int estacionamientoId) =>
        _context.GruposFinancieros.FirstOrDefault(g =>
            g.GrupoFinancieroId == id && g.EstacionamientoId == estacionamientoId);

    public bool ExisteNombreActivo(string nombre, int estacionamientoId, int? excludeId = null)
    {
        var normalizado = nombre.Trim().ToLower();
        var query = _context.GruposFinancieros.Where(g =>
            g.EstacionamientoId == estacionamientoId
            && g.Activo
            && g.Nombre.ToLower() == normalizado);
        if (excludeId.HasValue)
            query = query.Where(g => g.GrupoFinancieroId != excludeId);
        return query.Any();
    }

    public void Add(GrupoFinanciero grupo) => _context.GruposFinancieros.Add(grupo);

    public void SaveChanges() => _context.SaveChanges();
}
