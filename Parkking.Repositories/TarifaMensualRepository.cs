using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class TarifaMensualRepository
{
    private readonly EstacionamientoContext _context;

    public TarifaMensualRepository(EstacionamientoContext context) => _context = context;

    public List<TarifaMensual> GetAll(int estacionamientoId) =>
        _context.TarifasMensuales.Where(t => t.EstacionamientoId == estacionamientoId)
            .OrderByDescending(t => t.FechaHoraActualizacion).ToList();

    public List<TarifaMensual> GetByCombinacion(int tipoVehiculoId, int categoriaCocheraId, int estacionamientoId) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId && t.CategoriaCocheraId == categoriaCocheraId
                     && t.EstacionamientoId == estacionamientoId)
            .OrderByDescending(t => t.FechaHoraActualizacion).ToList();

    public void Add(TarifaMensual tarifa) => _context.TarifasMensuales.Add(tarifa);
    public void SaveChanges() => _context.SaveChanges();
}
