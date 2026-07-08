using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;
using Parkking.Models.Seguridad;

namespace Parkking.Models
{
    public class MovimientoCaja : IMultiTenant
    {
        [Key]
        public int MovimientoCajaId { get; set; }
        public int EstacionamientoId { get; set; }

        [ForeignKey(nameof(CajaMensual))]
        public int CajaMensualId { get; set; }
        public CajaMensual CajaMensual { get; set; } = null!;

        [ForeignKey(nameof(PagoMensual))]
        public int? PagoMensualId { get; set; }
        public PagoMensual? PagoMensual { get; set; }

        [Required]
        public TipoMovimiento Tipo { get; set; }

        [Required]
        [MaxLength(255)]
        public string Descripcion { get; set; } = string.Empty;

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

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoAnterior { get; set; } // Saldo de la caja antes de este movimiento

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoPosterior { get; set; } // Saldo final tras aplicar el monto

        [Required]
        [MaxLength(100)]
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; }

    }

}





