using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Repositories;

public class PagoRepository
{
    private readonly EstacionamientoContext _context;

    public PagoRepository(EstacionamientoContext context) => _context = context;

    public IDbContextTransaction BeginTransaction() => _context.Database.BeginTransaction();

    public Abono? GetAbonoActivo(int abonoId) =>
        _context.Abonos
            .Include(a => a.Cliente)
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.AbonoPlaza).ThenInclude(p => p!.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.Cuotas).ThenInclude(c => c.DetallesPago)
            .FirstOrDefault(a => a.AbonoId == abonoId && a.Activo);

    public Abono? GetAbonoConCuotas(int abonoId) =>
        _context.Abonos
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.AbonoPlaza).ThenInclude(p => p!.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.Cuotas).ThenInclude(c => c.Detalles)
            .Include(a => a.Cuotas)
                .ThenInclude(c => c.DetallesPago)
                .ThenInclude(d => d.Pago)
                .ThenInclude(p => p!.Recibo)
            .FirstOrDefault(a => a.AbonoId == abonoId);

    public Cuota? GetCuotaPorPeriodo(int abonoId, DateOnly periodoInicio) =>
        _context.Cuotas
            .Include(c => c.DetallesPago)
            .FirstOrDefault(c =>
                c.AbonoId == abonoId &&
                c.PeriodoInicio == periodoInicio &&
                c.Estado != EstadoCuota.Anulada);

    public Cuota? GetCuotaById(int cuotaId) =>
        _context.Cuotas
            .Include(c => c.DetallesPago)
            .Include(c => c.Detalles)
            .Include(c => c.Abono)
            .FirstOrDefault(c => c.CuotaId == cuotaId && c.Estado != EstadoCuota.Anulada);

    /// <summary>Recarga detalles de pago desde BD (evita saldo/estado stale en la misma unidad de trabajo).</summary>
    public void RecargarDetallesCuota(Cuota cuota)
    {
        _context.Entry(cuota).Collection(c => c.DetallesPago).Query().Load();
    }

    /// <summary>Carga líneas de liquidación (DetalleCuota) si no vinieron en el Include.</summary>
    public void CargarDetallesLiquidacion(Cuota cuota)
    {
        _context.Entry(cuota).Collection(c => c.Detalles).Query().Load();
    }

    public decimal GetMontoPagadoCuota(int cuotaId) =>
        _context.DetallesPago.Where(d => d.CuotaId == cuotaId).Sum(d => (decimal?)d.Monto) ?? 0;

    public Pago? GetPagoById(int pagoId) =>
        _context.Pagos
            .Include(p => p.Recibo)
            .Include(p => p.MetodoDePago)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(p => p.Abono).ThenInclude(a => a.Plazas).ThenInclude(pl => pl.Cochera)
            .Include(p => p.Abono).ThenInclude(a => a.Cliente)
            .FirstOrDefault(p => p.PagoId == pagoId);

    public void AddCuota(Cuota cuota) => _context.Cuotas.Add(cuota);
    public void AddPago(Pago pago) => _context.Pagos.Add(pago);
    public void AddDetallePago(DetallePago detalle) => _context.DetallesPago.Add(detalle);

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

    public Cliente? GetCliente(int id) => _context.Clientes.FirstOrDefault(c => c.ClienteId == id);

    public List<Abono> GetAbonosActivosByCliente(int clienteId) =>
        _context.Abonos
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.AbonoPlaza).ThenInclude(p => p!.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.Cuotas).ThenInclude(c => c.DetallesPago)
            .Where(a => a.ClienteId == clienteId && a.Activo)
            .ToList();

    public List<Abono> GetAbonosActivos() =>
        _context.Abonos
            .Include(a => a.Cliente)
            .Include(a => a.Plazas.Where(p => p.Activo)).ThenInclude(p => p.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.AbonoPlaza).ThenInclude(p => p!.Cochera).ThenInclude(c => c.CategoriaCochera)
            .Include(a => a.Cuotas).ThenInclude(c => c.DetallesPago)
            .Where(a => a.Activo)
            .ToList();

    public List<Pago> GetByAbono(int abonoId) =>
        _context.Pagos
            .Include(p => p.MetodoDePago)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(p => p.Abono).ThenInclude(a => a.Plazas).ThenInclude(pl => pl.Cochera)
            .Include(p => p.Abono).ThenInclude(a => a.Cliente)
            .Where(p => p.AbonoId == abonoId)
            .OrderByDescending(p => p.FechaHora)
            .ToList();

    public List<Pago> GetByCliente(int clienteId) =>
        _context.Pagos
            .Include(p => p.MetodoDePago)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(p => p.Abono).ThenInclude(a => a.Plazas).ThenInclude(pl => pl.Cochera)
            .Include(p => p.Abono).ThenInclude(a => a.Cliente)
            .Include(p => p.Abono).ThenInclude(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo).ThenInclude(v => v.TipoVehiculo)
            .Where(p => p.Abono.ClienteId == clienteId)
            .OrderByDescending(p => p.FechaHora)
            .ToList();

    public List<Pago> GetAll(DateOnly? desde, DateOnly? hasta)
    {
        var query = _context.Pagos
            .Include(p => p.MetodoDePago)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(p => p.Abono).ThenInclude(a => a.Plazas).ThenInclude(pl => pl.Cochera)
            .Include(p => p.Abono).ThenInclude(a => a.Cliente)
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(p => p.Detalles.Any(d => d.Cuota.PeriodoInicio >= desde));
        if (hasta.HasValue)
            query = query.Where(p => p.Detalles.Any(d => d.Cuota.PeriodoInicio <= hasta));

        return query.OrderByDescending(p => p.FechaHora).ToList();
    }

    /// <summary>Pagos del estacionamiento filtrados por fecha de cobro (FechaHora).</summary>
    public List<Pago> GetByFechaCobro(int estacionamientoId, DateOnly desde, DateOnly hasta)
    {
        var desdeDt = desde.ToDateTime(TimeOnly.MinValue);
        var hastaDt = hasta.ToDateTime(new TimeOnly(23, 59, 59));

        return _context.Pagos
            .Include(p => p.MetodoDePago)
            .Include(p => p.Recibo)
            .Include(p => p.Detalles).ThenInclude(d => d.Cuota)
            .Include(p => p.Abono).ThenInclude(a => a.Plazas.Where(pl => pl.Activo)).ThenInclude(pl => pl.Cochera)
            .Include(p => p.Abono).ThenInclude(a => a.Cliente)
            .Include(p => p.Abono).ThenInclude(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo)
            .Where(p => p.EstacionamientoId == estacionamientoId
                     && p.FechaHora >= desdeDt
                     && p.FechaHora <= hastaDt)
            .OrderByDescending(p => p.FechaHora)
            .ToList();
    }

    public void SaveChanges() => _context.SaveChanges();
}
