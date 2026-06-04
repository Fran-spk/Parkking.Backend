using API.Controllers;
using Parkking_backend.DTOs.Caja;
using Parkking_backend.Models;

public class GastoCocheraStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;
    private readonly int _cajaId;

    public GastoCocheraStrategy(RegistrarMovimientoRequest req, int cajaId)
    {
        _req = req;
        _cajaId = cajaId;
    }

    public MovimientoCaja CrearMovimiento()
    {
        return new MovimientoCaja
        {
            CajaMensualId = _cajaId,
            Tipo = TipoMovimiento.Egreso,
            TipoConcepto = TipoConcepto.GastoCochera,
            Descripcion = _req.Descripcion,
            Responsable = _req.Responsable,
            Monto = _req.Monto,
            FechaHora = DateTime.UtcNow
        };
    }
}