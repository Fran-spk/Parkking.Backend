using Parkking.Models;

namespace Parkking.Services.Movimientos;

public interface IMovimientoStrategy
{
    MovimientoCaja ConstruirMovimiento(int idCaja);

    void Validar();
}
