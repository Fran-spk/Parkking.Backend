using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Repositories;

public class DashboardRepository
{
    private readonly EstacionamientoContext _context;

    public DashboardRepository(EstacionamientoContext context) => _context = context;

    public List<Cochera> GetCocherasConAbonos(int estacionamientoId) =>
        _context.Cocheras
            .Include(c => c.CategoriaCochera)
            .Include(c => c.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Abono).ThenInclude(a => a.Cliente)
            .Include(c => c.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Abono).ThenInclude(a => a.Cuotas).ThenInclude(cu => cu.DetallesPago)
            .Include(c => c.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Abono)
                .ThenInclude(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Where(c => c.EstacionamientoId == estacionamientoId && c.Activo)
            .ToList();

    public List<Abono> GetAbonosActivos(int estacionamientoId) =>
        _context.Abonos
            .Include(a => a.Cliente)
            .Include(a => a.Cuotas).ThenInclude(c => c.DetallesPago)
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Where(a => a.EstacionamientoId == estacionamientoId && a.Activo)
            .ToList();

    public decimal ObtenerTarifaVigente(
        int tipoVehiculoId,
        int categoriaId,
        PeriodicidadCobro periodicidadCobro,
        int estacionamientoId,
        DateTime referencia) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId
                     && t.CategoriaCocheraId == categoriaId
                     && t.PeriodicidadCobro == periodicidadCobro
                     && t.EstacionamientoId == estacionamientoId
                     && t.FechaHoraActualizacion <= referencia)
            .OrderByDescending(t => t.FechaHoraActualizacion)
            .Select(t => t.Precio)
            .FirstOrDefault();

    public List<Pago> GetPagosDesde(DateOnly desde, int estacionamientoId) =>
        _context.Pagos
            .Include(p => p.Abono)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Where(p => p.Abono.EstacionamientoId == estacionamientoId &&
                        p.Detalles.Any(d => d.Cuota.PeriodoInicio >= desde))
            .ToList();
}
