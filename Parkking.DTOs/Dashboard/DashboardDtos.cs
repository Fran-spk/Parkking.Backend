namespace Parkking.DTOs.Dashboard;

public class CocheraOcupanteDto
{
    public int AbonoId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? Patente { get; set; }
    public string? VehiculoModelo { get; set; }
}

public class CocheraStatusDto
{
    public int CocheraId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    /// <summary>Compat: nombres unidos con " · ".</summary>
    public string? ClienteNombre { get; set; }
    /// <summary>Compat: patentes unidas con " · ".</summary>
    public string? Patente { get; set; }
    /// <summary>Compat: primer modelo (si hay varios, ver <see cref="Ocupantes"/>).</summary>
    public string? VehiculoModelo { get; set; }
    public bool MultipleOcupacion { get; set; }
    public int AbonosActivos { get; set; }
    public List<CocheraOcupanteDto> Ocupantes { get; set; } = new();
}

public class AlertaDeudorDto
{
    public int AbonoId { get; set; }
    public int AbonoCocheraId { get => AbonoId; set => AbonoId = value; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public string CocheraNumero { get; set; } = string.Empty;
    public int DiasAtraso { get; set; }
    /// <summary>Compat / estimado. Preferir <see cref="SaldoTotal"/>.</summary>
    public decimal? PrecioAcordado { get; set; }
    public decimal SaldoTotal { get; set; }
    public int PeriodosAdeudados { get; set; }
}

public class DistribucionVehiculoDto
{
    public string TipoVehiculo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class HistoricoIngresosDto
{
    public string Mes { get; set; } = string.Empty;
    public decimal Monto { get; set; }
}

public class DetalleIngresosKpiDto
{
    public decimal TotalRecaudadoMensualReal { get; set; }
    public decimal TotalEstimadoProyeccion { get; set; }
    public double DiferenciaPorcentaje { get; set; }
}

public class DashboardAbonosDto
{
    public double PorcentajeOcupacion { get; set; }
    public int CocherasOcupadas { get; set; }
    public int CocherasTotales { get; set; }
    public int CantidadAbonosActivos { get; set; }
    public List<CocheraStatusDto> EstadoCocheras { get; set; } = new();
    public List<AlertaDeudorDto> AlertasDeudores { get; set; } = new();
    public List<DistribucionVehiculoDto> DistribucionVehiculos { get; set; } = new();
    public List<HistoricoIngresosDto> GraficoIngresos { get; set; } = new();
    public DetalleIngresosKpiDto DetalleIngresos { get; set; } = new();
}
