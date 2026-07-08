using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class DashboardRepository
{
    private readonly EstacionamientoContext _context;

    public DashboardRepository(EstacionamientoContext context) => _context = context;

    public List<Cochera> GetCocherasConAbonos(int estacionamientoId) =>
        _context.Cocheras
            .Include(c => c.CategoriaCochera)
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.Cliente)
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.TipoVehiculo)
            .Include(c => c.Abonos.Where(a => a.Activo)).ThenInclude(a => a.PagosMensuales)
            .Where(c => c.EstacionamientoId == estacionamientoId && c.Activo)
            .ToList();

    public decimal ObtenerTarifaVigente(int tipoVehiculoId, int categoriaId, int estacionamientoId, DateTime referencia) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId && t.CategoriaCocheraId == categoriaId
                     && t.EstacionamientoId == estacionamientoId && t.FechaHoraActualizacion <= referencia)
            .OrderByDescending(t => t.FechaHoraActualizacion)
            .Select(t => t.Precio)
            .FirstOrDefault();

    public List<PagoMensual> GetPagosDesde(DateOnly desde, int estacionamientoId) =>
        _context.PagosMensuales
            .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
            .Where(p => p.AbonoCochera.Cochera.EstacionamientoId == estacionamientoId && p.Mes >= desde)
            .ToList();
}
