namespace Parkking.DTOs.Caja;

public class MovimientoCajaDto
{
    public int MovimientoCajaId { get; set; }
    public DateTime FechaHora { get; set; }
    public decimal Monto { get; set; }
    public int Tipo { get; set; }
    public string TipoDescripcion { get; set; } = string.Empty;
    public int TipoConcepto { get; set; }
    public string TipoConceptoDescripcion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Usuario { get; set; }
    public decimal SaldoAnterior { get; set; }
    public decimal SaldoPosterior { get; set; }

}
