using Parkking.DTOs.Caja;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;
using Parkking.Services.Movimientos;
using Mapster;

namespace Parkking.Services;

public class CajaMensualService
{
    private readonly CajaMensualRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public CajaMensualService(
        CajaMensualRepository repository,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    private CajaMensual GetCaja(int year, int month) =>
        _repository.GetByMes(year, month, TenantId)
        ?? throw new Exception("No hay caja para ese mes");


    public List<MovimientoCaja> GetMovimientos( //traer usuario
        int year,
        int month,
        TipoMovimiento? tipo = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var caja = GetCaja(year, month);

        var movimientos = caja.Movimientos.AsQueryable();

        if (tipo.HasValue)
            movimientos = movimientos.Where(m => m.Tipo == tipo.Value);

        if (desde.HasValue)
            movimientos = movimientos.Where(m => m.FechaHora >= desde.Value);

        if (hasta.HasValue)
            movimientos = movimientos.Where(m => m.FechaHora <= hasta.Value);

        return movimientos
            .OrderByDescending(m => m.FechaHora)
            .ToList();
    }

    public ResumenCajaDto GetResumen(int year, int month)
    {
        var caja = GetCaja(year, month);

        var dto = caja.Adapt<ResumenCajaDto>();

        dto.CantidadMovimientos = caja.Movimientos.Count;

        return dto;
    }
    public List<ConceptoCajaDto> GetConceptos(
        int year,
        int month)
    {
        var caja = GetCaja(year, month);

        return caja.Movimientos
            .GroupBy(m => m.TipoConcepto)
            .Select(g => new ConceptoCajaDto
            {
                TipoConcepto = (TipoConcepto)(int)g.Key,
                TipoConceptoDescripcion = g.Key.GetDescription(),
                CantidadMovimientos = g.Count(),
                Total = g.Sum(x => x.Monto)
            })
            .OrderBy(c => c.TipoConcepto)
            .ToList();
    }

    public void Close(int year, int month)
    {
        var caja = GetCaja(year, month);

        if (caja.Cerrada)
            throw new Exception("La caja ya está cerrada");

        caja.Cerrada = true;

        _repository.SaveChanges();
    }

    public MovimientoCaja RegistrarMovimiento(IMovimientoStrategy estrategia)
    {
        estrategia.Validar();

        var hoy = DateTime.UtcNow;

        var caja = _repository.GetCajaSinMovimientos(hoy.Year, hoy.Month, TenantId);

        if (caja == null)
        {
            caja = new CajaMensual
            {
                EstacionamientoId = TenantId,
                Mes = new DateOnly(hoy.Year, hoy.Month, 1),
                Cerrada = false
            };

            _repository.AddCaja(caja);
            _repository.SaveChanges();
        }

        if (caja.Cerrada)
            throw new Exception($"La caja operativa actual ({hoy:MM/yyyy}) se encuentra cerrada por administración.");

        var movimiento = estrategia.ConstruirMovimiento(caja.CajaMensualId);

        movimiento.FechaHora = hoy;
        movimiento.UsuarioId = _estacionamiento.GetUsuarioActual().Id;

        movimiento.SaldoAnterior = caja.Saldo;

        movimiento.SaldoPosterior = movimiento.Tipo == TipoMovimiento.Ingreso
            ? caja.Saldo + movimiento.Monto
            : caja.Saldo - movimiento.Monto;

        caja.RegistrarImpactoFinanciero(
            movimiento.Tipo,
            movimiento.Monto);

        _repository.AddMovimiento(movimiento);

        _repository.SaveChanges();

        return movimiento;
    }

    public List<MovimientoCaja> GetMovimientosByAbono(
        int abonoCocheraId)
    {
        return _repository.GetMovimientosByAbono(abonoCocheraId);
    }
}