using API.Controllers;
using Parkking_backend.DTOs.Caja;

namespace Parkking_backend.Models.Movimientos_Strategy
{
    public static class MovimientoFactory
    {
        public static IMovimientoStrategy Crear(
            RegistrarMovimientoRequest req,
            int cajaId
            )
        {
            return req.TipoConcepto switch
            {
                TipoConcepto.GastoCochera => new GastoCocheraStrategy(req, cajaId),
                TipoConcepto.ReintegroCliente => new ReintegroClienteStrategy(req, cajaId),
                TipoConcepto.CargoCliente => new CargoClienteStrategy(req, cajaId),
                _ => throw new Exception("Tipo de movimiento no soportado")
            };
        }
    }
}
