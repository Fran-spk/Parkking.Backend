using System.ComponentModel.DataAnnotations;
using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Services.Movimientos;

public class GastoCocheraStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;

    public void Validar()
    {
        if (_req.Monto <= 0)
            throw new ValidationException(
                "El monto debe ser mayor a cero."
            );

        if (_req.Descripcion== null)
            throw new ValidationException(
                "El gasto debe tener una descripcion."
            );
    }
    public GastoCocheraStrategy(RegistrarMovimientoRequest req)
    {
        _req = req;
    }

    public MovimientoCaja ConstruirMovimiento(int cajaId) => new()
    {
        CajaMensualId = cajaId,
        Tipo = TipoMovimiento.Egreso,
        TipoConcepto = TipoConcepto.GastoCochera,
        Descripcion = _req.Descripcion,
        Monto = _req.Monto,
        FechaHora = DateTime.UtcNow
    };
}
