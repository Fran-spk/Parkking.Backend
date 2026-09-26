using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class MensajeRepository
{
    private readonly EstacionamientoContext _context;

    public MensajeRepository(EstacionamientoContext context) => _context = context;

    public void Add(Mensaje mensaje) => _context.Mensajes.Add(mensaje);

    public void SaveChanges() => _context.SaveChanges();

    /// <summary>Listado auditável del tenant (más recientes primero).</summary>
    public List<Mensaje> List(
        int estacionamientoId,
        string? tipo = null,
        string? estado = null,
        int take = 100)
    {
        var q = _context.Mensajes.Where(m => m.EstacionamientoId == estacionamientoId);

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var t = tipo.Trim();
            q = q.Where(m => m.Tipo == t);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var e = estado.Trim();
            q = q.Where(m => m.Estado == e);
        }

        return q
            .OrderByDescending(m => m.Fecha)
            .Take(Math.Clamp(take, 1, 500))
            .ToList();
    }

    public List<Mensaje> ListByAbono(int estacionamientoId, int abonoId, int take = 50) =>
        _context.Mensajes
            .Where(m => m.EstacionamientoId == estacionamientoId && m.AbonoId == abonoId)
            .OrderByDescending(m => m.Fecha)
            .Take(take)
            .ToList();

    public List<Mensaje> ListByRecibo(int estacionamientoId, int reciboId, int take = 50) =>
        _context.Mensajes
            .Where(m => m.EstacionamientoId == estacionamientoId && m.ReciboId == reciboId)
            .OrderByDescending(m => m.Fecha)
            .Take(take)
            .ToList();
}
