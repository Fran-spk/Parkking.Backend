using Parkking_backend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using MODELO;

public class MovimientoCaja
{
    [Key]
    public int MovimientoCajaId { get; set; }

    [ForeignKey("CajaMensual")]
    public int CajaMensualId { get; set; }
    public CajaMensual CajaMensual { get; set; }

    [ForeignKey("PagoMensual")]
    public int? PagoMensualId { get; set; }
    public PagoMensual? PagoMensual { get; set; }

    [Required]
    public TipoMovimiento Tipo { get; set; }

    [Required]
    [MaxLength(255)]
    public string Descripcion { get; set; }

    [MaxLength(100)]
    public string? Responsable { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Required]
    public DateTime FechaHora { get; set; }

    public int? AbonoCocheraId { get; set; }
    public AbonoCochera? Abono { get; set; }
    public TipoConcepto TipoConcepto { get; set; }
}

public enum TipoMovimiento
{
    Ingreso,
    Egreso
}
//un movimiento puede ser por un pago mensual => logica ya resuelta ,no te piedo esto
//un movimineot puede serr por un gasto operativo => tambien lo resolvimos en el apartado cajas on un boton
//un movimiento por ultimo puede ser por un cargo a un cliente en su abono ejemplo: rompio un control, ajuste de abono a un mes etc y un reintegro: cuando el estacionamiento le entrega plata: de esta manera existe un balance por cliente tambien