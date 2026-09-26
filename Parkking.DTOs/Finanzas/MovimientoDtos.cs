namespace Parkking.DTOs.Finanzas;

public class MovimientoDto
{
    public int MovimientoId { get; set; }
    public int EstacionamientoId { get; set; }
    public int CuentaCorrienteEstacionamientoId { get; set; }
    public int? ClienteId { get; set; }
    public int? CuentaCorrienteClienteId { get; set; }
    public int? PagoId { get; set; }
    public decimal Importe { get; set; }
    public int Tipo { get; set; }
    public string TipoDescripcion { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public int UsuarioId { get; set; }
    public List<int> GrupoFinancieroIds { get; set; } = new();
}

public class AuditoriaMovimientoDto
{
    public int AuditoriaMovimientoId { get; set; }
    public DateTime FechaHora { get; set; }
    public int UsuarioId { get; set; }
    public string? Ip { get; set; }
    public string? UserAgent { get; set; }
    public string Detalle { get; set; } = string.Empty;
}

public class MovimientoDetalleDto : MovimientoDto
{
    public List<AuditoriaMovimientoDto> Auditorias { get; set; } = new();
}

public class CrearReintegroRequest
{
    public int? ClienteId { get; set; }
    public int? AbonoId { get; set; }
    public int? TipoGastoId { get; set; }
    public decimal Importe { get; set; }
    public string Beneficiario { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string Medio { get; set; } = string.Empty;
    public List<int> GrupoFinancieroIds { get; set; } = new();
}

public class CuentaCorrienteConsultaDto
{
    public int? CuentaCorrienteId { get; set; }
    public int EstacionamientoId { get; set; }
    public int? ClienteId { get; set; }
    public decimal Saldo { get; set; }
    public bool Existe { get; set; }
    public List<MovimientoDto> Movimientos { get; set; } = new();
}

public class GrupoFinancieroPeriodoDto
{
    public int GrupoFinancieroId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos { get; set; }
    public decimal Neto { get; set; }
    public List<MovimientoDto> Movimientos { get; set; } = new();
}
