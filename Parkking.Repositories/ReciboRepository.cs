using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class ReciboRepository
{
    private readonly EstacionamientoContext _context;

    public ReciboRepository(EstacionamientoContext context) => _context = context;

    private IQueryable<Recibo> WithIncludes() =>
        _context.Recibos
            .Include(r => r.Pago).ThenInclude(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(r => r.Pago).ThenInclude(p => p.Abono).ThenInclude(a => a.Cliente);

    public Recibo? GetById(int reciboId) =>
        WithIncludes().FirstOrDefault(r => r.ReciboId == reciboId);

    public Recibo? GetByPagoId(int pagoId) =>
        WithIncludes().FirstOrDefault(r => r.PagoId == pagoId);

    public List<Recibo> Listar(DateTime? desde, DateTime? hasta, string? cliente, string? q)
    {
        var query = WithIncludes().AsQueryable();

        if (desde.HasValue)
            query = query.Where(r => r.FechaEmision >= desde.Value);

        if (hasta.HasValue)
        {
            var fin = hasta.Value.Date.AddDays(1);
            query = query.Where(r => r.FechaEmision < fin);
        }

        if (!string.IsNullOrWhiteSpace(cliente))
        {
            var c = cliente.Trim().ToLower();
            query = query.Where(r => r.ClienteNombre.ToLower().Contains(c));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(r =>
                r.ClienteNombre.ToLower().Contains(term) ||
                (r.CocherasLabel != null && r.CocherasLabel.ToLower().Contains(term)) ||
                (r.PatentesLabel != null && r.PatentesLabel.ToLower().Contains(term)) ||
                r.Numero.ToString().Contains(term));
        }

        return query
            .OrderByDescending(r => r.FechaEmision)
            .ThenByDescending(r => r.Numero)
            .Take(200)
            .ToList();
    }

    public int ProximoNumero(int estacionamientoId)
    {
        var max = _context.Recibos
            .IgnoreQueryFilters()
            .Where(r => r.EstacionamientoId == estacionamientoId)
            .Select(r => (int?)r.Numero)
            .Max();
        return (max ?? 0) + 1;
    }

    public void Add(Recibo recibo) => _context.Recibos.Add(recibo);

    public void SaveChanges() => _context.SaveChanges();
}
