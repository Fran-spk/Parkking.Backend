using Parkking.Models;
using Parkking.Models.Enums;

namespace Parkking.DTOs.Caja;

public class RegistrarMovimientoRequest
{
    public TipoConcepto TipoConcepto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? ClienteNombre { get; set; }
    public int? AbonoId { get; set; }
    public int? AbonoCocheraId { get => AbonoId; set => AbonoId = value; }
}
