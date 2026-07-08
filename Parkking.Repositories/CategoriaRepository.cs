using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class CategoriaRepository
{
    private readonly EstacionamientoContext _context;

    public CategoriaRepository(EstacionamientoContext context) => _context = context;

    public List<CategoriaCochera> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var query = _context.CategoriasCochera.Where(c => c.EstacionamientoId == estacionamientoId);
        if (!includeInactivos) query = query.Where(c => c.Activo);
        return query.OrderBy(c => c.Nombre).ToList();
    }

    public CategoriaCochera? GetById(int id, int estacionamientoId) =>
        _context.CategoriasCochera.FirstOrDefault(c => c.CategoriaCocheraId == id && c.EstacionamientoId == estacionamientoId);

    public bool ExisteNombre(string nombre, int estacionamientoId, int? excludeId = null)
    {
        var query = _context.CategoriasCochera.Where(c =>
            c.Nombre == nombre && c.EstacionamientoId == estacionamientoId && c.Activo);
        if (excludeId.HasValue) query = query.Where(c => c.CategoriaCocheraId != excludeId);
        return query.Any();
    }

    public bool TieneCocherasActivas(int categoriaId) =>
        _context.Cocheras.Any(c => c.CategoriaCocheraId == categoriaId && c.Activo);

    public void Add(CategoriaCochera categoria) => _context.CategoriasCochera.Add(categoria);
    public void SaveChanges() => _context.SaveChanges();
}
