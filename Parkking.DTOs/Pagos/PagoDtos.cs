namespace Parkking.DTOs.Pagos;

public class RegistrarPagoRequest
{
    public int AbonoCocheraId { get; set; }
    public string Mes { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal? Recargo { get; set; }
    public string? Observacion { get; set; }
    public string? MercadoPagoId { get; set; }
}

public class PagoSugeridoDto
{
    public decimal Monto { get; set; }
    public decimal Recargo { get; set; }
    public bool AplicaProporcional { get; set; }
    public bool EsPrecioAcordado { get; set; }
    public DateOnly MesDate { get; set; }
}

public class MesImpagoDto
{
    public int AbonoCocheraId { get; set; }
    public string NumeroCochera { get; set; } = string.Empty;
    public string TipoVehiculo { get; set; } = string.Empty;
    public string Mes { get; set; } = string.Empty;
    public DateOnly MesDate { get; set; }
    public decimal Monto { get; set; }
    public decimal Recargo { get; set; }
    public decimal Total { get; set; }
}

public class DeudaClienteDto
{
    public int ClienteId { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public List<MesImpagoDto> MesesImpagos { get; set; } = new();
    public decimal TotalAdeudado => MesesImpagos?.Sum(m => m.Total) ?? 0;
}
