using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Services.Movimientos;

public class PagoMensualStrategy : IMovimientoStrategy
{
    private readonly PagoMensual _pago;
    private readonly AbonoCochera _abono;

    public PagoMensualStrategy(PagoMensual pago, AbonoCochera abono)
    {
        _pago = pago;
        _abono = abono;
    }

    public void Validar()
    {
        // Regla de negocio: No se puede registrar un movimiento de pago con monto cero o negativo
        if (_pago.Monto <= 0)
            throw new ValidationException("El monto del pago debe ser mayor a cero.");
    }

    public MovimientoCaja ConstruirMovimiento(int cajaId) => new()
    {
        CajaMensualId = cajaId,
        PagoMensualId = _pago.PagoMensualId,
        Tipo = TipoMovimiento.Ingreso,
        TipoConcepto = TipoConcepto.PagoMensual,
        // Usamos CultureInfo para formatear el mes contable de forma amigable (ej: "octubre 2026")
        Descripcion = $"Cobro abono cochera {_abono.Cochera.Numero} - {_pago.Mes.ToString("MMMM yyyy", new CultureInfo("es-AR"))}",
        Monto = _pago.Monto + (_pago.Recargo ?? 0),
        EstacionamientoId = _abono.EstacionamientoId // Mantenemos consistencia del Tenant
    };
}