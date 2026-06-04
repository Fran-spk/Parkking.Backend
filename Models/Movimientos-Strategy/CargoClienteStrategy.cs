using API.Controllers;
using Parkking_backend.DTOs.Caja;
using Parkking_backend.Models;

public class CargoClienteStrategy : IMovimientoStrategy
{
    private readonly RegistrarMovimientoRequest _req;
    private readonly int _cajaId;

    public CargoClienteStrategy(RegistrarMovimientoRequest req, int cajaId)
    {
        _req = req;
        _cajaId = cajaId;
    }

    public MovimientoCaja CrearMovimiento()
    {
        return new MovimientoCaja
        {
            CajaMensualId = _cajaId,
            Tipo = TipoMovimiento.Ingreso,
            TipoConcepto = TipoConcepto.CargoCliente,
            Descripcion = $"Cargo a {_req.ClienteNombre} - {_req.Descripcion}",
            Monto = _req.Monto,
            FechaHora = DateTime.UtcNow,
            AbonoCocheraId = _req.AbonoCocheraId
        };
    }
}