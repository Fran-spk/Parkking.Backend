namespace Parkking_backend.DTOs.Dashboard;


public class CocheraStatusDto
{
    public int CocheraId { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // "Libre" o "Ocupado-Abono"
    public string? ClienteNombre { get; set; }
    public string? Patente { get; set; }
    public string? VehiculoModelo { get; set; }
}

public class AlertaDeudorDto
{
    public int AbonoCocheraId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteTelefono { get; set; }
    public string CocheraNumero { get; set; } = string.Empty;
    public int DiasAtraso { get; set; }
    public decimal? PrecioAcordado { get; set; }
}

public class DistribucionVehiculoDto
{
    public string TipoVehiculo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class HistoricoIngresosDto
{
    public string Mes { get; set; } = string.Empty; // Ej: "Mar", "Abr"
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

    // LA PROPIEDAD CLAVE PARA EL FRONTEND MODIFICADA
    public DetalleIngresosKpiDto DetalleIngresos { get; set; } = new();
}