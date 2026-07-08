using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class PagoRepository
{
    private readonly EstacionamientoContext _context;

    public PagoRepository(EstacionamientoContext context) => _context = context;

    public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();

    public AbonoCochera? GetAbonoActivo(int abonoCocheraId) =>
        _context.AbonoCocheras.Include(a => a.Cochera)
            .FirstOrDefault(a => a.AbonoCocheraId == abonoCocheraId && a.Activo);

    public bool ExistePago(int abonoCocheraId, int year, int month) =>
        _context.PagosMensuales.Any(p => p.AbonoCocheraId == abonoCocheraId && p.Mes.Year == year && p.Mes.Month == month);

    public void AddPago(PagoMensual pago) => _context.PagosMensuales.Add(pago);

    public CajaMensual? GetCaja(int year, int month, int estacionamientoId) =>
        _context.CajasMensuales.FirstOrDefault(c =>
            c.Mes.Year == year && c.Mes.Month == month && c.EstacionamientoId == estacionamientoId);

    public void AddCaja(CajaMensual caja) => _context.CajasMensuales.Add(caja);
    public void AddMovimiento(MovimientoCaja movimiento) => _context.MovimientosCaja.Add(movimiento);

    public Estacionamiento? GetEstacionamiento(int id) =>
        _context.Estacionamientos.FirstOrDefault(e => e.EstacionamientoId == id);

    public decimal ObtenerTarifaVigente(int tipoVehiculoId, int categoriaId, int estacionamientoId, DateTime referencia) =>
        _context.TarifasMensuales
            .Where(t => t.TipoVehiculoId == tipoVehiculoId && t.CategoriaCocheraId == categoriaId
                     && t.EstacionamientoId == estacionamientoId && t.FechaHoraActualizacion <= referencia)
            .OrderByDescending(t => t.FechaHoraActualizacion)
            .Select(t => t.Precio)
            .FirstOrDefault();

    public Cliente? GetCliente(int id) => _context.Clientes.FirstOrDefault(c => c.ClienteId == id);

    public List<AbonoCochera> GetAbonosActivosByCliente(int clienteId) =>
        _context.AbonoCocheras.Include(a => a.Cochera).Include(a => a.TipoVehiculo).Include(a => a.PagosMensuales)
            .Where(a => a.ClienteId == clienteId && a.Activo).ToList();

    public List<PagoMensual> GetByAbono(int abonoId) =>
        _context.PagosMensuales.Where(p => p.AbonoCocheraId == abonoId).OrderByDescending(p => p.Mes).ToList();

    public List<PagoMensual> GetByCliente(int clienteId) =>
        _context.PagosMensuales
            .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
            .Include(p => p.AbonoCochera).ThenInclude(a => a.TipoVehiculo)
            .Where(p => p.AbonoCochera.ClienteId == clienteId)
            .OrderByDescending(p => p.Mes).ToList();

    public List<PagoMensual> GetAll(DateOnly? desde, DateOnly? hasta)
    {
        var query = _context.PagosMensuales
            .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
            .Include(p => p.AbonoCochera).ThenInclude(a => a.Cliente)
            .AsQueryable();
        if (desde.HasValue) query = query.Where(p => p.Mes >= desde);
        if (hasta.HasValue) query = query.Where(p => p.Mes <= hasta);
        return query.OrderByDescending(p => p.FechaHoraCarga).ToList();
    }

    public List<PagoMensual> GetPagosDesde(DateOnly desde, int estacionamientoId) =>
        _context.PagosMensuales
            .Include(p => p.AbonoCochera).ThenInclude(a => a.Cochera)
            .Where(p => p.AbonoCochera.Cochera.EstacionamientoId == estacionamientoId && p.Mes >= desde)
            .ToList();

    public void SaveChanges() => _context.SaveChanges();
}
