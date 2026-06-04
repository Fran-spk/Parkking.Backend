using MODELO;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class PagoMensual
{
    [Key]
    public int PagoMensualId { get; set; }

    [ForeignKey("AbonoCochera")]
    public int AbonoCocheraId { get; set; }
    public AbonoCochera AbonoCochera { get; set; }

    [Required]
    public DateOnly Mes { get; set; }

    [Required]
    public DateTime FechaHoraCarga { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Recargo { get; set; }

    [MaxLength(255)]
    public string? Observacion { get; set; }

    public ICollection<MovimientoCaja> Movimientos { get; set; }
        = new List<MovimientoCaja>();
    public string? MercadoPagoId { get; set; }
}