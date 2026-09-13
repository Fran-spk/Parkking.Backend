using System.ComponentModel.DataAnnotations;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Services.Movimientos;

public class PagoAbonoStrategy : IMovimientoStrategy
{
    private readonly Pago _pago;
    private readonly Abono _abono;
    private readonly Cuota _cuota;

    public PagoAbonoStrategy(Pago pago, Abono abono, Cuota cuota)
    {
        _pago = pago;
        _abono = abono;
        _cuota = cuota;
    }

    public void Validar()
    {
        if (_pago.MontoTotal <= 0)
            throw new ValidationException("El monto del pago debe ser mayor a cero.");
    }

    public MovimientoCaja ConstruirMovimiento(int cajaId)
    {
        var numeroCochera = _abono.Plazas.FirstOrDefault(p => p.Activo)?.Cochera?.Numero ?? "N/A";
        var periodoLabel = _cuota.PeriodoInicio == _cuota.PeriodoFin
            ? _cuota.PeriodoInicio.ToString("dd/MM/yyyy")
            : $"{_cuota.PeriodoInicio:dd/MM} – {_cuota.PeriodoFin:dd/MM/yyyy}";

        return new()
        {
            CajaMensualId = cajaId,
            PagoId = _pago.PagoId,
            AbonoId = _abono.AbonoId,
            Tipo = TipoMovimiento.Ingreso,
            TipoConcepto = TipoConcepto.PagoAbono,
            Descripcion = $"Cobro abono cochera {numeroCochera} - {periodoLabel}",
            Monto = _pago.MontoTotal,
            EstacionamientoId = _abono.EstacionamientoId
        };
    }
}
