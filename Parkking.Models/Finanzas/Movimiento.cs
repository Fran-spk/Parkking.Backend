using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Parkking.Models.Enums;

namespace Parkking.Models.Finanzas;

/// <summary>
/// Hecho financiero. Pertenece siempre a un estacionamiento.
/// El cliente es opcional. Un solo importe con signo (el del estacionamiento).
/// </summary>
public class Movimiento : IMultiTenant
{
    [Key]
    public int MovimientoId { get; set; }

    public int EstacionamientoId { get; set; }

    [ForeignKey(nameof(Cliente))]
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    [ForeignKey(nameof(Pago))]
    public int? PagoId { get; set; }
    public Pago? Pago { get; set; }

    public int? AbonoId { get; set; }
    public Abono? Abono { get; set; }

    public int? TipoGastoId { get; set; }
    public TipoGasto? TipoGasto { get; set; }

    [ForeignKey(nameof(CuentaCorrienteEstacionamiento))]
    public int CuentaCorrienteEstacionamientoId { get; set; }
    public CuentaCorrienteEstacionamiento CuentaCorrienteEstacionamiento { get; set; } = null!;

    [ForeignKey(nameof(CuentaCorrienteCliente))]
    public int? CuentaCorrienteClienteId { get; set; }
    public CuentaCorrienteCliente? CuentaCorrienteCliente { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Importe { get; set; }

    public TipoMovimiento Tipo { get; set; }

    [Required]
    [MaxLength(255)]
    public string Concepto { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    public int UsuarioId { get; set; }

    [MaxLength(45)]
    public string? Ip { get; set; }

    [MaxLength(400)]
    public string? UserAgent { get; set; }

    public ICollection<MovimientoGrupoFinanciero> Grupos { get; set; } = new List<MovimientoGrupoFinanciero>();
    public ICollection<AuditoriaMovimiento> Auditorias { get; set; } = new List<AuditoriaMovimiento>();

    public void Imputar(CuentaCorrienteEstacionamiento cuentaEstacionamiento, CuentaCorrienteCliente? cuentaCliente)
    {
        CuentaCorrienteEstacionamiento = cuentaEstacionamiento;
        CuentaCorrienteEstacionamientoId = cuentaEstacionamiento.CuentaCorrienteEstacionamientoId;
        cuentaEstacionamiento.Aplicar(this);

        if (ClienteId is null) return;

        if (cuentaCliente is null)
            throw new InvalidOperationException("El movimiento tiene cliente y no tiene cuenta corriente de cliente.");

        CuentaCorrienteCliente = cuentaCliente;
        CuentaCorrienteClienteId = cuentaCliente.CuentaCorrienteClienteId;
        cuentaCliente.Aplicar(this);
    }

    public void AgregarGrupo(int grupoFinancieroId)
    {
        if (Grupos.Any(g => g.GrupoFinancieroId == grupoFinancieroId)) return;

        Grupos.Add(new MovimientoGrupoFinanciero
        {
            EstacionamientoId = EstacionamientoId,
            GrupoFinancieroId = grupoFinancieroId
        });
    }

    public void AgregarGrupos(IEnumerable<int> grupoFinancieroIds)
    {
        foreach (var grupoId in grupoFinancieroIds.Distinct())
            AgregarGrupo(grupoId);
    }

    public void Auditar(string detalle)
    {
        Auditorias.Add(new AuditoriaMovimiento
        {
            EstacionamientoId = EstacionamientoId,
            FechaHora = FechaHora,
            UsuarioId = UsuarioId,
            Ip = Ip,
            UserAgent = UserAgent,
            Detalle = detalle
        });
    }
}
