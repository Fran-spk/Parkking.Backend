using Parkking.DTOs.Caja;
using Parkking.Models;
using Parkking.Services.Movimientos;

namespace Parkking.Services;

public class MovimientoService
{
    private readonly CajaMensualService _cajaService;

    public MovimientoService(CajaMensualService cajaService)
    {
        _cajaService = cajaService;
    }

    public MovimientoCaja RegistrarCargoCliente(
        RegistrarMovimientoRequest request)
    {
        var strategy = new CargoClienteStrategy(request);

        return _cajaService.RegistrarMovimiento(strategy);
    }

    public MovimientoCaja RegistrarReintegroCliente(
        RegistrarMovimientoRequest request)
    {
        var strategy = new ReintegroClienteStrategy(request);

        return _cajaService.RegistrarMovimiento(strategy);
    }

    public MovimientoCaja RegistrarGastoEstacionamiento(
        RegistrarMovimientoRequest request)
    {
        var strategy = new GastoCocheraStrategy(request);

        return _cajaService.RegistrarMovimiento(strategy);
    }
}