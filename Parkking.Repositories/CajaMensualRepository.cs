using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Repositories;

public class CajaMensualRepository
{
    private readonly EstacionamientoContext _context;

    public CajaMensualRepository(EstacionamientoContext context) => _context = context;

    public CajaMensual? GetByMes(int year, int month, int estacionamientoId) =>
        _context.CajasMensuales
            .Include(c => c.Movimientos).ThenInclude(m=>m.Usuario)
            .FirstOrDefault(c => c.Mes.Year == year && c.Mes.Month == month && c.EstacionamientoId == estacionamientoId);

    public CajaMensual? GetCajaSinMovimientos(int year, int month, int estacionamientoId) =>
        _context.CajasMensuales
            .FirstOrDefault(c => c.Mes.Year == year && c.Mes.Month == month && c.EstacionamientoId == estacionamientoId);
    public void AddCaja(CajaMensual caja) => _context.CajasMensuales.Add(caja);

    public List<MovimientoCaja> GetMovimientosByAbono(int abonoCocheraId) =>
        _context.MovimientosCaja
            .Where(m => m.AbonoCocheraId == abonoCocheraId &&
                        (m.TipoConcepto == TipoConcepto.CargoCliente || m.TipoConcepto == TipoConcepto.ReintegroCliente))
            .OrderByDescending(m => m.FechaHora).ToList();

    public void AddMovimiento(MovimientoCaja movimiento) => _context.MovimientosCaja.Add(movimiento);
    public void SaveChanges() => _context.SaveChanges();
}
