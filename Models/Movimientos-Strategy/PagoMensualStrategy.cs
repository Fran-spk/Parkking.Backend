using MODELO;
using System.Globalization;

namespace Parkking_backend.Models
{
    public class PagoMensualStrategy : IMovimientoStrategy
    {
        private readonly PagoMensual _pago;
        private readonly AbonoCochera _abono;
        private readonly int _cajaId;

        public PagoMensualStrategy(PagoMensual pago, AbonoCochera abono, int cajaId)
        {
            _pago = pago;
            _abono = abono;
            _cajaId = cajaId;
        }

        public MovimientoCaja CrearMovimiento()
        {
            return new MovimientoCaja
            {
                CajaMensualId = _cajaId,
                PagoMensualId = _pago.PagoMensualId,
                Tipo = TipoMovimiento.Ingreso,
                TipoConcepto = TipoConcepto.PagoMensual,
                Descripcion = $"Cobro abono cochera {_abono.Cochera.Numero} - {_pago.Mes.ToString("MMMM yyyy", new CultureInfo("es-AR"))}",
                Monto = _pago.Monto + (_pago.Recargo ?? 0),
                FechaHora = DateTime.UtcNow
            };
        }
    }
    public interface IMovimientoStrategy
    {
        MovimientoCaja CrearMovimiento();
    }

    public enum TipoConcepto
    {
        PagoMensual = 0,
        ReintegroCliente = 1,
        CargoCliente = 2,    
        GastoCochera = 3
    }
}
