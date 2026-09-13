using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Repositories;

public class TarifaMensualRepository
{
    private readonly EstacionamientoContext _context;

    public TarifaMensualRepository(EstacionamientoContext context) => _context = context;

    public List<TarifaMensual> GetAll(int estacionamientoId) =>
        _context.TarifasMensuales.Where(t => t.EstacionamientoId == estacionamientoId)
            .OrderByDescending(t => t.FechaHoraActualizacion).ToList();

    public List<TarifaMensual> GetByCombinacion(
        int tipoVehiculoId,
        int categoriaCocheraId,
        PeriodicidadCobro periodicidadCobro,
        int estacionamientoId) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId
                     && t.CategoriaCocheraId == categoriaCocheraId
                     && t.PeriodicidadCobro == periodicidadCobro
                     && t.EstacionamientoId == estacionamientoId)
            .OrderByDescending(t => t.FechaHoraActualizacion).ToList();

    /// <summary>Tarifa vigente (última actualización ≤ referencia) para la combinación completa.</summary>
    public TarifaMensual? GetVigente(
        int tipoVehiculoId,
        int categoriaCocheraId,
        PeriodicidadCobro periodicidadCobro,
        int estacionamientoId,
        DateTime referencia) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId
                     && t.CategoriaCocheraId == categoriaCocheraId
                     && t.PeriodicidadCobro == periodicidadCobro
                     && t.EstacionamientoId == estacionamientoId
                     && t.FechaHoraActualizacion <= referencia)
            .OrderByDescending(t => t.FechaHoraActualizacion)
            .FirstOrDefault();

    public void Add(TarifaMensual tarifa) => _context.TarifasMensuales.Add(tarifa);
    public void SaveChanges() => _context.SaveChanges();
}
