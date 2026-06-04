using API.Controllers;
using Parkking_backend.DTOs.Caja;
using Parkking_backend.Models;

public class ReintegroClienteStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;
    private readonly int _cajaId;

    public ReintegroClienteStrategy(RegistrarMovimientoRequest req, int cajaId)
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
            TipoConcepto = TipoConcepto.ReintegroCliente,
            Descripcion = $"Reintegro cliente {_req.ClienteNombre}",
            Monto = _req.Monto,
            FechaHora = DateTime.UtcNow,
            AbonoCocheraId = _req.AbonoCocheraId
        };
    }
}