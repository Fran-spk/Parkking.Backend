using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models.Enums;
using Parkking.Models.Finanzas;

namespace Parkking.Repositories;

public class ReglaAsignacionRepository
{
    private readonly EstacionamientoContext _context;

    public ReglaAsignacionRepository(EstacionamientoContext context) => _context = context;

    public List<ReglaAsignacion> GetAll(int estacionamientoId, bool includeInactivas)
    {
        var query = _context.ReglasAsignacion
            .Include(r => r.GrupoFinanciero)
            .Where(r => r.EstacionamientoId == estacionamientoId);
        if (!includeInactivas) query = query.Where(r => r.Activa);
        return query.OrderBy(r => r.ReglaAsignacionId).ToList();
    }

    public ReglaAsignacion? GetById(int id, int estacionamientoId) =>
        _context.ReglasAsignacion
            .Include(r => r.GrupoFinanciero)
            .FirstOrDefault(r => r.ReglaAsignacionId == id && r.EstacionamientoId == estacionamientoId);

    public bool ExisteAbono(int abonoId, int estacionamientoId) =>
        _context.Abonos.Any(a => a.AbonoId == abonoId && a.EstacionamientoId == estacionamientoId);

    public bool ExisteCliente(int clienteId, int estacionamientoId) =>
        _context.Clientes.Any(c => c.ClienteId == clienteId && c.EstacionamientoId == estacionamientoId && c.Activo);

    public bool ExisteTipoGasto(int tipoGastoId, int estacionamientoId) =>
        _context.TiposGasto.Any(t =>
            t.TipoGastoId == tipoGastoId && t.EstacionamientoId == estacionamientoId && t.Activo);

    public List<int> GruposQueMatchean(int estacionamientoId, int? clienteId, int? tipoGastoId)
    {
        if (clienteId is not > 0 && tipoGastoId is not > 0)
            return new List<int>();

        return _context.ReglasAsignacion
            .Where(r => r.EstacionamientoId == estacionamientoId && r.Activa && r.GrupoFinanciero.Activo)
            .Where(r =>
                (clienteId != null && r.Criterio == CriterioRegla.Cliente && r.ClienteId == clienteId)
                || (tipoGastoId != null && r.Criterio == CriterioRegla.TipoGasto && r.TipoGastoId == tipoGastoId))
            .Select(r => r.GrupoFinancieroId)
            .Distinct()
            .ToList();
    }

    public void Add(ReglaAsignacion regla) => _context.ReglasAsignacion.Add(regla);

    public void SaveChanges() => _context.SaveChanges();
}
