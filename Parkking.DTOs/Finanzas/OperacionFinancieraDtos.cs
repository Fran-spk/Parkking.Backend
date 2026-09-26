namespace Parkking.DTOs.Finanzas;

public class CrearCargoRequest
{
    public int ClienteId { get; set; }
    public int AbonoId { get; set; }
    public decimal Importe { get; set; }
    public string Concepto { get; set; } = string.Empty;
}

public class CargoDto
{
    public int CargoId { get; set; }
    public int ClienteId { get; set; }
    public int? AbonoId { get; set; }
    public decimal Importe { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

public class ReintegroDto
{
    public int ReintegroId { get; set; }
    public int? ClienteId { get; set; }
    public int? AbonoId { get; set; }
    public int MovimientoId { get; set; }
    public decimal Importe { get; set; }
    public string Beneficiario { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string Medio { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
}

public class CrearGastoRequest
{
    public int TipoGastoId { get; set; }
    public decimal Importe { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public List<int> GrupoFinancieroIds { get; set; } = new();
}

public class CrearAjusteRequest
{
    public int? ClienteId { get; set; }
    public int? AbonoId { get; set; }
    public int? TipoGastoId { get; set; }
    /// <summary>Siempre positivo. El signo lo define <see cref="AFavorDelCliente"/>.</summary>
    public decimal Importe { get; set; }
    /// <summary>Si es true, el cliente queda a favor y la cuenta del estacionamiento resta.</summary>
    public bool AFavorDelCliente { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public List<int> GrupoFinancieroIds { get; set; } = new();
}

public class OperacionFinancieraDto
{
    public int? CargoId { get; set; }
    public int? MovimientoId { get; set; }
    public int? AjusteFinancieroId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Importe { get; set; }
    public int? ClienteId { get; set; }
    public List<int> GrupoFinancieroIds { get; set; } = new();
}
