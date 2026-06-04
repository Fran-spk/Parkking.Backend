using Parkking_backend.Models;

namespace Parkking_backend.DTOs.Caja
{
    public static class MovimientoCajaMapper
    {
        public static MovimientoCajaDto ToDto(MovimientoCaja m)
        {
            return new MovimientoCajaDto
            {
                MovimientoCajaId = m.MovimientoCajaId,
                FechaHora = m.FechaHora,
                Monto = m.Monto,

                Tipo = (int)m.Tipo,
                TipoDescripcion = m.Tipo == TipoMovimiento.Ingreso ? "Ingreso" : "Gasto",

                TipoConcepto = (int)m.TipoConcepto,
                TipoConceptoDescripcion = m.TipoConcepto switch
                {
                    TipoConcepto.PagoMensual => "Pago mensual",
                    TipoConcepto.ReintegroCliente => "Reintegro cliente",
                    TipoConcepto.GastoCochera=> "Gasto cochera",
                    TipoConcepto.CargoCliente => "Cargo cliente",
                    _ => "Otro"
                },

                Descripcion = m.Descripcion,
                Responsable = m.Responsable
            };
        }
    }
}
