using System.ComponentModel.DataAnnotations;
using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Services.Movimientos;

public class CargoClienteStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;


    public CargoClienteStrategy(RegistrarMovimientoRequest req)
    {
        _req = req;
    }

    public void Validar()
    {
        if (_req.Monto <= 0)
            throw new ValidationException(
                "El monto debe ser mayor a cero."
            );

        if (_req.AbonoId == null)
            throw new ValidationException(
                "El cargo debe estar asociado a un abono."
            );
    }

    public MovimientoCaja ConstruirMovimiento(int idCaja) => new()
    {
        CajaMensualId = idCaja,
        Tipo = TipoMovimiento.Ingreso,
        TipoConcepto = TipoConcepto.CargoCliente,
        Descripcion = $"Cargo a {_req.ClienteNombre} - {_req.Descripcion}",
        Monto = _req.Monto,
        FechaHora = DateTime.UtcNow,
        AbonoId = _req.AbonoId
    };
}
