using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.Services.Movimientos;

public class ReintegroClienteStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;

    public ReintegroClienteStrategy(RegistrarMovimientoRequest req)
    {
        _req = req;
    }

    public void Validar()
    {
        // Regla de negocio: No se puede registrar un movimiento de pago con monto cero o negativo
        if (_req.Monto <= 0)
            throw new ValidationException("El reintegro debe ser mayor a cero.");
    }
    public MovimientoCaja ConstruirMovimiento( int cajaId) => new()
    {
        CajaMensualId = cajaId,
        Tipo = TipoMovimiento.Egreso,
        TipoConcepto = TipoConcepto.ReintegroCliente,
        Descripcion = $"Reintegro cliente {_req.ClienteNombre}",
        Monto = _req.Monto,
        FechaHora = DateTime.UtcNow,
        AbonoCocheraId = _req.AbonoCocheraId
    };
}
