using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Repositories;

public class AbonoRepository
{
    private readonly EstacionamientoContext _context;

    public AbonoRepository(EstacionamientoContext context) => _context = context;

    public IQueryable<Abono> WithIncludes(IQueryable<Abono> query) =>
        query.Include(a => a.Cliente)
             .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.CategoriaCochera)
             .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
             .Include(a => a.AbonoVehiculos).ThenInclude(av => av.AbonoPlaza).ThenInclude(p => p!.Cochera).ThenInclude(c => c.CategoriaCochera);

    public List<Abono> GetAll(int estacionamientoId) =>
        WithIncludes(_context.Abonos)
            .Where(a => a.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.AbonoId)
            .ToList();

    public List<Abono> GetActivos(int estacionamientoId) =>
        WithIncludes(_context.Abonos)
            .Where(a => a.Activo && a.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.AbonoId)
            .ToList();

    public List<Abono> GetByCliente(int clienteId, int estacionamientoId) =>
        WithIncludes(_context.Abonos)
            .Where(a => a.ClienteId == clienteId && a.EstacionamientoId == estacionamientoId)
            .OrderBy(a => a.AbonoId)
            .ToList();

    public List<Abono> GetByCochera(int cocheraId) =>
        WithIncludes(_context.Abonos)
            .Where(a => a.Plazas.Any(p => p.CocheraId == cocheraId && p.Activo))
            .OrderByDescending(a => a.FechaInicio)
            .ToList();

    public Abono? GetByPatente(string patente, int estacionamientoId)
    {
        var q = patente.Trim().ToUpper();
        return WithIncludes(_context.Abonos)
            .Where(a => a.EstacionamientoId == estacionamientoId &&
                        a.AbonoVehiculos.Any(av =>
                            av.Vehiculo.Patente.Replace(" ", "").Replace("-", "").ToUpper() == q))
            .OrderByDescending(a => a.Activo)
            .ThenByDescending(a => a.FechaInicio)
            .FirstOrDefault();
    }

    /// <summary>Búsqueda parcial por patente (case-insensitive). Prioriza abonos activos.</summary>
    public List<Abono> BuscarPorPatente(string termino, int estacionamientoId)
    {
        var q = termino.Trim().ToUpper();
        return WithIncludes(_context.Abonos)
            .Where(a => a.EstacionamientoId == estacionamientoId &&
                        a.AbonoVehiculos.Any(av =>
                            av.Vehiculo.Patente.Replace(" ", "").Replace("-", "").ToUpper().Contains(q)))
            .OrderByDescending(a => a.Activo)
            .ThenByDescending(a => a.FechaInicio)
            .Take(30)
            .ToList();
    }

    public Abono? GetActivoById(int id) =>
        WithIncludes(_context.Abonos).FirstOrDefault(a => a.AbonoId == id && a.Activo);

    public Abono? GetActivoWithPlazas(int id) =>
        _context.Abonos
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.VehiculosPermitidos)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo)
            .FirstOrDefault(a => a.AbonoId == id && a.Activo);

    public Abono? LoadWithIncludes(int id) =>
        WithIncludes(_context.Abonos).FirstOrDefault(a => a.AbonoId == id);

    public Cliente? GetCliente(int id) => _context.Clientes.Find(id);

    public Cochera? GetCochera(int id) =>
        _context.Cocheras
            .Include(c => c.VehiculosPermitidos)
            .FirstOrDefault(c => c.CocheraId == id);

    public Cochera? GetCocheraDestino(int cocheraId, int estacionamientoId) =>
        _context.Cocheras
            .Include(c => c.Plazas).ThenInclude(p => p.Abono)
            .Include(c => c.VehiculosPermitidos)
            .FirstOrDefault(c => c.CocheraId == cocheraId && c.EstacionamientoId == estacionamientoId && c.Activo);

    public AbonoPlaza? GetPlaza(int abonoPlazaId) =>
        _context.AbonoPlazas
            .Include(p => p.Abono)
            .Include(p => p.Cochera).ThenInclude(c => c.VehiculosPermitidos)
            .Include(p => p.VehiculosFijos).ThenInclude(av => av.Vehiculo)
            .FirstOrDefault(p => p.AbonoPlazaId == abonoPlazaId);

    public AbonoVehiculo? GetAbonoVehiculo(int abonoVehiculoId) =>
        _context.AbonoVehiculos
            .Include(av => av.Vehiculo)
            .Include(av => av.AbonoPlaza)
            .Include(av => av.Abono).ThenInclude(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.VehiculosPermitidos)
            .FirstOrDefault(av => av.AbonoVehiculoId == abonoVehiculoId);

    public Vehiculo? GetVehiculo(int vehiculoId) =>
        _context.Vehiculos
            .Include(v => v.AbonoVehiculo)
            .FirstOrDefault(v => v.VehiculoId == vehiculoId);

    public Vehiculo? GetVehiculoByPatente(string patente, int clienteId, int estacionamientoId) =>
        _context.Vehiculos
            .Include(v => v.AbonoVehiculo)
            .FirstOrDefault(v =>
                v.EstacionamientoId == estacionamientoId &&
                v.ClienteId == clienteId &&
                v.Patente.Replace(" ", "").Replace("-", "").ToUpper() == patente);

    /// <summary>Vehículo del estacionamiento con esa patente (cualquier cliente).</summary>
    public Vehiculo? GetVehiculoByPatenteEnEstacionamiento(string patenteNormalizada, int estacionamientoId) =>
        _context.Vehiculos
            .Include(v => v.AbonoVehiculo).ThenInclude(av => av!.Abono)
            .FirstOrDefault(v =>
                v.EstacionamientoId == estacionamientoId &&
                v.Patente.Replace(" ", "").Replace("-", "").ToUpper() == patenteNormalizada);

    public bool ExistePlazaActiva(int abonoId, int cocheraId) =>
        _context.AbonoPlazas.Any(p => p.AbonoId == abonoId && p.CocheraId == cocheraId && p.Activo);

    public bool PatenteEnAbono(int abonoId, string patenteNormalizada, int? excludeAbonoVehiculoId = null) =>
        _context.AbonoVehiculos.Any(av =>
            av.AbonoId == abonoId &&
            av.Vehiculo.Patente.Replace(" ", "").Replace("-", "").ToUpper() == patenteNormalizada &&
            (excludeAbonoVehiculoId == null || av.AbonoVehiculoId != excludeAbonoVehiculoId));

    /// <summary>True si la patente está vinculada a un abono activo del estacionamiento.</summary>
    public bool PatenteEnAbonoActivo(
        int estacionamientoId,
        string patenteNormalizada,
        int? excludeAbonoId = null,
        int? excludeVehiculoId = null) =>
        _context.AbonoVehiculos.Any(av =>
            av.Abono.EstacionamientoId == estacionamientoId &&
            av.Abono.Activo &&
            av.Vehiculo.Patente.Replace(" ", "").Replace("-", "").ToUpper() == patenteNormalizada &&
            (excludeAbonoId == null || av.AbonoId != excludeAbonoId) &&
            (excludeVehiculoId == null || av.VehiculoId != excludeVehiculoId));

    public bool VehiculoAsignadoAAbonoActivo(int vehiculoId, int? excludeAbonoId = null) =>
        _context.AbonoVehiculos.Any(av =>
            av.VehiculoId == vehiculoId &&
            av.Abono.Activo &&
            (excludeAbonoId == null || av.AbonoId != excludeAbonoId));

    public void Add(Abono abono) => _context.Abonos.Add(abono);
    public void AddCuota(Cuota cuota) => _context.Cuotas.Add(cuota);
    public void AddVehiculo(Vehiculo vehiculo) => _context.Vehiculos.Add(vehiculo);
    public void AddAbonoVehiculo(AbonoVehiculo abonoVehiculo) => _context.AbonoVehiculos.Add(abonoVehiculo);
    public void AddPlaza(AbonoPlaza plaza) => _context.AbonoPlazas.Add(plaza);
    public void RemoveAbonoVehiculo(AbonoVehiculo abonoVehiculo) => _context.AbonoVehiculos.Remove(abonoVehiculo);

    public DatosEstacionamiento? GetEstacionamiento(int id) =>
        _context.Estacionamientos.FirstOrDefault(e => e.EstacionamientoId == id);

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

    public void SaveChanges() => _context.SaveChanges();
}
