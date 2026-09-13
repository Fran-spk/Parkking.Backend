using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class MetodoDePagoRepository
{
    private readonly EstacionamientoContext _context;

    public MetodoDePagoRepository(EstacionamientoContext context) => _context = context;

    public List<MetodoDePago> GetAll(int estacionamientoId, bool includeInactivos)
    {
        var query = _context.MetodosDePago.Where(m => m.EstacionamientoId == estacionamientoId);
        if (!includeInactivos) query = query.Where(m => m.Activo);
        return query.OrderBy(m => m.Nombre).ToList();
    }

    public MetodoDePago? GetById(int id, int estacionamientoId) =>
        _context.MetodosDePago.FirstOrDefault(m =>
            m.MetodoDePagoId == id && m.EstacionamientoId == estacionamientoId);

    public bool ExisteNombre(string nombre, int estacionamientoId, int? excludeId = null)
    {
        var normalizado = nombre.Trim().ToLower();
        var query = _context.MetodosDePago.Where(m =>
            m.EstacionamientoId == estacionamientoId
            && m.Activo
            && m.Nombre.ToLower() == normalizado);
        if (excludeId.HasValue)
            query = query.Where(m => m.MetodoDePagoId != excludeId);
        return query.Any();
    }

    public bool TienePagos(int metodoDePagoId) =>
        _context.Pagos.Any(p => p.MetodoDePagoId == metodoDePagoId);

    public void SeedDefaultsSiFaltan(int estacionamientoId, IEnumerable<string> nombres)
    {
        var existentes = _context.MetodosDePago
            .IgnoreQueryFilters()
            .Where(m => m.EstacionamientoId == estacionamientoId)
            .Select(m => m.Nombre.ToLower())
            .ToList()
            .ToHashSet();

        var faltantes = nombres
            .Where(n => !existentes.Contains(n.Trim().ToLower()))
            .Select(n => new MetodoDePago
            {
                EstacionamientoId = estacionamientoId,
                Nombre = n.Trim(),
                Activo = true
            })
            .ToList();

        if (faltantes.Count == 0) return;
        _context.MetodosDePago.AddRange(faltantes);
        _context.SaveChanges();
    }

    public void Add(MetodoDePago metodo) => _context.MetodosDePago.Add(metodo);

    public void SaveChanges() => _context.SaveChanges();
}
