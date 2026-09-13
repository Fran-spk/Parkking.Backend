namespace Parkking.DTOs.Recibos;

public class ReciboDto
{
    public int ReciboId { get; set; }
    public int Numero { get; set; }
    public string NumeroFormateado { get; set; } = string.Empty;
    public int PagoId { get; set; }
    public int AbonoId { get; set; }
    public DateTime FechaEmision { get; set; }
    public decimal Monto { get; set; }
    public decimal Recargo { get; set; }
    public decimal Total { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? CocherasLabel { get; set; }
    public string? PatentesLabel { get; set; }
    public string? PeriodosLabel { get; set; }
    public string? Cobrador { get; set; }
    public string? MetodoPagoLabel { get; set; }
    public string? Observacion { get; set; }
    public bool Anulado { get; set; }
    public string? MotivoAnulacion { get; set; }
}

public class AnularReciboRequest
{
    public string? Motivo { get; set; }
}
