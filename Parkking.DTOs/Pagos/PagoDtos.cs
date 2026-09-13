using Parkking.Models.Enums;

namespace Parkking.DTOs.Pagos;

public class PagoDto
{
    public int PagoId { get; set; }
    public int AbonoId { get; set; }

    public DateOnly Mes { get; set; }
    public DateTime FechaHoraCarga { get; set; }
    public decimal Monto { get; set; }
    public decimal? Recargo { get; set; }
    public string? Observacion { get; set; }
    public string? MercadoPagoId { get; set; }

    public int? MetodoDePagoId { get; set; }
    public string? MetodoDePagoNombre { get; set; }

    public int? CuotaId { get; set; }
    public DateOnly? PeriodoInicio { get; set; }
    public DateOnly? PeriodoFin { get; set; }

    public string? ClienteNombre { get; set; }
    public string? NumeroCochera { get; set; }

    public int? ReciboId { get; set; }
    public string? ReciboNumero { get; set; }
}

public class DetallePagoDto
{
    public int DetallePagoId { get; set; }
    public int PagoId { get; set; }
    public int CuotaId { get; set; }
    public decimal Monto { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public string PeriodoLabel { get; set; } = string.Empty;
    public EstadoCuota EstadoCuota { get; set; }
    public decimal MontoCuota { get; set; }
    public decimal SaldoCuota { get; set; }
}

public class PagoConDetallesDto : PagoDto
{
    public List<DetallePagoDto> Detalles { get; set; } = new();
}

public class CuotaPendienteDto
{
    public int? CuotaId { get; set; }
    public int AbonoId { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public string PeriodoLabel { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal Saldo { get; set; }
    public EstadoCuota Estado { get; set; }
    public bool EsPrecioAcordado { get; set; }
    public List<ComponenteTarifaDto> ComponentesTarifa { get; set; } = new();
    /// <summary>Desglose persistido (o vacío si la cuota aún no se materializó).</summary>
    public List<DetalleCuotaDto> DetallesLiquidacion { get; set; } = new();
}

/// <summary>Línea de liquidación de una cuota (snapshot al crear).</summary>
public class DetalleCuotaDto
{
    public int DetalleCuotaId { get; set; }
    public int CuotaId { get; set; }
    public int? TarifaMensualId { get; set; }
    public TipoDetalleCuota Tipo { get; set; }
    public string TipoLabel { get; set; } = string.Empty;
    public int? VehiculoId { get; set; }
    public int? CocheraId { get; set; }
    public string? Patente { get; set; }
    public string? CocheraNumero { get; set; }
    public string? TipoVehiculoNombre { get; set; }
    public string? CategoriaNombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Importe { get; set; }
}

/// <summary>Cobro imputado a una cuota (para timeline).</summary>
public class CuotaCobroDto
{
    public int DetallePagoId { get; set; }
    public int PagoId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }
    public string? Observacion { get; set; }
    public decimal? RecargoPago { get; set; }
    public int? ReciboId { get; set; }
    public string? ReciboNumero { get; set; }
}

/// <summary>Período del abono con estado y cobros aplicados.</summary>
public class CuotaTimelineDto : CuotaPendienteDto
{
    public bool EsFuturo { get; set; }
    public List<CuotaCobroDto> Cobros { get; set; } = new();
}

public class RegistrarDetallePagoRequest
{
    /// <summary>Si existe, se usa esta cuota.</summary>
    public int? CuotaId { get; set; }

    /// <summary>Alternativa: fecha dentro del período (yyyy-MM-dd).</summary>
    public string? PeriodoInicio { get; set; }

    public decimal Monto { get; set; }
}

public class RegistrarPagoRequest
{
    public int AbonoId { get; set; }

    /// <summary>Compat: un solo período si Detalles está vacío.</summary>
    public string Mes { get; set; } = string.Empty;
    public decimal Monto { get; set; }

    /// <summary>Desglose del cobro (1 o N cuotas). Si viene vacío, se usa Mes+Monto.</summary>
    public List<RegistrarDetallePagoRequest> Detalles { get; set; } = new();

    public decimal? Recargo { get; set; }
    public string? Observacion { get; set; }
    public string? MercadoPagoId { get; set; }

    /// <summary>Obligatorio al cobrar.</summary>
    public int MetodoDePagoId { get; set; }
}

public class PagoSugeridoDto
{
    public decimal Monto { get; set; }
    public decimal Recargo { get; set; }
    public bool AplicaProporcional { get; set; }
    public bool EsPrecioAcordado { get; set; }
    public DateOnly MesDate { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public List<ComponenteTarifaDto> ComponentesTarifa { get; set; } = new();
}

public class MesImpagoDto
{
    public int AbonoId { get; set; }
    public string NumeroCochera { get; set; } = string.Empty;
    public string TipoVehiculo { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;
    public DateOnly MesDate { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public decimal Monto { get; set; }
    public decimal Recargo { get; set; }
    public decimal Total { get; set; }
    public decimal Saldo { get; set; }
}

public class DeudaClienteDto
{
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public List<MesImpagoDto> MesesImpagos { get; set; } = new();
    public decimal TotalAdeudado => MesesImpagos?.Sum(m => m.Total) ?? 0;
}

/// <summary>Fila de la cola de cobro: un período/cuota con saldo en un abono activo.</summary>
public class DeudaPendienteDto
{
    public int AbonoId { get; set; }
    public int? CuotaId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public string CocherasLabel { get; set; } = string.Empty;
    public string? PatentesLabel { get; set; }
    public DateOnly PeriodoInicio { get; set; }
    public DateOnly PeriodoFin { get; set; }
    public string PeriodoLabel { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal MontoPagado { get; set; }
    public decimal Saldo { get; set; }
    public EstadoCuota Estado { get; set; }
    public int DiasAtraso { get; set; }
}
