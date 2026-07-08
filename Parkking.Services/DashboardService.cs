using System.Globalization;
using Parkking.DTOs.Dashboard;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class DashboardService
{
    private readonly DashboardRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public DashboardService(DashboardRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    public DashboardAbonosDto GetSummary()
    {
        var estacionamientoId = _estacionamiento.EstacionamientoId;
        var cocheras = _repository.GetCocherasConAbonos(estacionamientoId);
        var abonosActivos = cocheras.SelectMany(c => c.Abonos).ToList();

        decimal ObtenerTarifa(int tipoVehiculoId, int categoriaId) =>
            _repository.ObtenerTarifaVigente(tipoVehiculoId, categoriaId, estacionamientoId, DateTime.Now);

        var historicoIngresos = GenerarHistoricoIngresos(estacionamientoId);
        var totalRecaudadoRealMes = historicoIngresos.LastOrDefault()?.Monto ?? 0;

        return new DashboardAbonosDto
        {
            CocherasTotales = cocheras.Count,
            CocherasOcupadas = cocheras.Count(c => c.Abonos.Any(a => a.Activo)),
            PorcentajeOcupacion = CalcularPorcentajeOcupacion(cocheras),
            CantidadAbonosActivos = abonosActivos.Count,
            EstadoCocheras = GenerarMapaCocheras(cocheras),
            AlertasDeudores = GenerarAlertasDeudores(abonosActivos, cocheras),
            DistribucionVehiculos = GenerarDistribucionVehiculos(abonosActivos),
            GraficoIngresos = historicoIngresos,
            DetalleIngresos = CalcularDetalleIngresos(abonosActivos, totalRecaudadoRealMes, ObtenerTarifa)
        };
    }

    private static double CalcularPorcentajeOcupacion(List<Cochera> cocheras)
    {
        var totales = cocheras.Count;
        var ocupadas = cocheras.Count(c => c.Abonos.Any(a => a.Activo));
        return totales > 0 ? Math.Round((double)ocupadas / totales * 100, 1) : 0;
    }

    private static List<CocheraStatusDto> GenerarMapaCocheras(List<Cochera> cocheras) =>
        cocheras.Select(c =>
        {
            var abonoVigente = c.Abonos.FirstOrDefault(a => a.Activo);
            return new CocheraStatusDto
            {
                CocheraId = c.CocheraId,
                Numero = c.Numero,
                Estado = abonoVigente != null ? "Ocupado-Abono" : "Libre",
                ClienteNombre = abonoVigente?.Cliente?.Nombre,
                Patente = abonoVigente?.Patente,
                VehiculoModelo = abonoVigente?.ModeloVehiculo
            };
        }).OrderBy(c => c.Numero).ToList();

    private static List<AlertaDeudorDto> GenerarAlertasDeudores(List<AbonoCochera> abonosActivos, List<Cochera> cocheras) =>
        abonosActivos.Where(a => a.TieneDeuda()).Select(abono => new AlertaDeudorDto
        {
            AbonoCocheraId = abono.AbonoCocheraId,
            ClienteNombre = abono.Cliente?.Nombre ?? "Sin Nombre",
            ClienteTelefono = abono.Cliente?.Telefono,
            CocheraNumero = cocheras.FirstOrDefault(c => c.CocheraId == abono.CocheraId)?.Numero ?? "",
            DiasAtraso = abono.ObtenerDiasAtraso(),
            PrecioAcordado = abono.PrecioAcordado
        }).OrderByDescending(a => a.DiasAtraso).ToList();

    private static List<DistribucionVehiculoDto> GenerarDistribucionVehiculos(List<AbonoCochera> abonosActivos)
    {
        var total = abonosActivos.Count;
        return abonosActivos.GroupBy(a => a.TipoVehiculo.Nombre ?? "Otros")
            .Select(g => new DistribucionVehiculoDto
            {
                TipoVehiculo = g.Key,
                Cantidad = g.Count(),
                Porcentaje = total > 0 ? Math.Round((double)g.Count() / total * 100, 1) : 0
            }).ToList();
    }

    private List<HistoricoIngresosDto> GenerarHistoricoIngresos(int estacionamientoId)
    {
        var fechaLimite = DateOnly.FromDateTime(DateTime.Today.AddMonths(-5));
        var primerDiaLimite = new DateOnly(fechaLimite.Year, fechaLimite.Month, 1);
        var pagosReales = _repository.GetPagosDesde(primerDiaLimite, estacionamientoId);

        var ingresosAgrupados = pagosReales
            .GroupBy(p => new { p.Mes.Year, p.Mes.Month })
            .Select(g => new { g.Key.Year, MesNum = g.Key.Month, Total = g.Sum(p => p.Monto + (p.Recargo ?? 0)) })
            .ToList();

        var grafico = new List<HistoricoIngresosDto>();
        for (var i = 5; i >= 0; i--)
        {
            var fechaMes = DateTime.Today.AddMonths(-i);
            var nombreMes = fechaMes.ToString("MMM", new CultureInfo("es-ES"));
            nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1).Replace(".", "");
            var total = ingresosAgrupados.FirstOrDefault(x => x.Year == fechaMes.Year && x.MesNum == fechaMes.Month)?.Total ?? 0;
            grafico.Add(new HistoricoIngresosDto { Mes = nombreMes, Monto = total });
        }
        return grafico;
    }

    private static DetalleIngresosKpiDto CalcularDetalleIngresos(
        List<AbonoCochera> abonosActivos, decimal totalRecaudadoReal, Func<int, int, decimal> obtenerTarifa)
    {
        var totalEstimado = abonosActivos.Sum(a => a.CalcularMontoMensual(obtenerTarifa));
        double diferenciaPorcentaje = 0;
        if (totalEstimado > 0)
            diferenciaPorcentaje = Math.Round((double)((totalRecaudadoReal - totalEstimado) / totalEstimado) * 100, 1);

        return new DetalleIngresosKpiDto
        {
            TotalRecaudadoMensualReal = totalRecaudadoReal,
            TotalEstimadoProyeccion = totalEstimado,
            DiferenciaPorcentaje = diferenciaPorcentaje
        };
    }
}
