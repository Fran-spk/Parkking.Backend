using System.Globalization;
using Parkking.DTOs.Dashboard;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
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
        var abonosActivos = _repository.GetAbonosActivos(estacionamientoId);

        var historicoIngresos = GenerarHistoricoIngresos(estacionamientoId);
        var totalRecaudadoRealMes = historicoIngresos.LastOrDefault()?.Monto ?? 0;

        return new DashboardAbonosDto
        {
            CocherasTotales = cocheras.Count,
            CocherasOcupadas = cocheras.Count(c => c.Plazas.Any(p => p.Activo && p.Abono.Activo)),
            PorcentajeOcupacion = CalcularPorcentajeOcupacion(cocheras),
            CantidadAbonosActivos = abonosActivos.Count,
            EstadoCocheras = GenerarMapaCocheras(cocheras),
            AlertasDeudores = GenerarAlertasDeudores(abonosActivos),
            DistribucionVehiculos = GenerarDistribucionVehiculos(abonosActivos),
            GraficoIngresos = historicoIngresos,
            DetalleIngresos = CalcularDetalleIngresos(abonosActivos, totalRecaudadoRealMes)
        };
    }

    private static double CalcularPorcentajeOcupacion(List<Cochera> cocheras)
    {
        var totales = cocheras.Count;
        var ocupadas = cocheras.Count(c => c.Plazas.Any(p => p.Activo && p.Abono.Activo));
        return totales > 0 ? Math.Round((double)ocupadas / totales * 100, 1) : 0;
    }

    private static List<CocheraStatusDto> GenerarMapaCocheras(List<Cochera> cocheras) =>
        cocheras.Select(c =>
        {
            var plazasActivas = c.Plazas
                .Where(p => p.Activo && p.Abono != null && p.Abono.Activo)
                .OrderBy(p => p.AbonoPlazaId)
                .ToList();

            var ocupantes = plazasActivas.Select(plaza =>
            {
                var vehiculo = plaza.Abono.AbonoVehiculos?
                    .FirstOrDefault(av => av.AbonoPlazaId == plaza.AbonoPlazaId)
                    ?.Vehiculo
                    ?? plaza.Abono.AbonoVehiculos?
                        .FirstOrDefault(av => av.AbonoPlazaId == null)
                        ?.Vehiculo
                    ?? plaza.Abono.AbonoVehiculos?.FirstOrDefault()?.Vehiculo;

                return new CocheraOcupanteDto
                {
                    AbonoId = plaza.Abono.AbonoId,
                    ClienteNombre = plaza.Abono.Cliente?.Nombre ?? "Sin nombre",
                    Patente = vehiculo?.Patente,
                    VehiculoModelo = vehiculo?.ModeloVehiculo,
                };
            }).ToList();

            return new CocheraStatusDto
            {
                CocheraId = c.CocheraId,
                Numero = c.Numero,
                Estado = ocupantes.Count > 0 ? "Ocupado-Abono" : "Libre",
                MultipleOcupacion = c.MultipleOcupacion,
                AbonosActivos = ocupantes.Count,
                Ocupantes = ocupantes,
                ClienteNombre = ocupantes.Count == 0
                    ? null
                    : string.Join(" · ", ocupantes.Select(o => o.ClienteNombre).Distinct()),
                Patente = ocupantes.Count == 0
                    ? null
                    : string.Join(" · ", ocupantes
                        .Select(o => o.Patente)
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Distinct()!),
                VehiculoModelo = ocupantes.FirstOrDefault()?.VehiculoModelo,
            };
        }).OrderBy(c => c.Numero).ToList();

    private static List<AlertaDeudorDto> GenerarAlertasDeudores(List<Abono> abonosActivos)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var alertas = new List<AlertaDeudorDto>();

        foreach (var abono in abonosActivos)
        {
            var montoBase = abono.PrecioAcordado ?? 0;
            decimal saldoTotal = 0;
            var periodos = 0;
            DateOnly? primerImpago = null;

            foreach (var (inicio, _) in PeriodicidadHelper.EnumerarPeriodos(
                         abono.FechaInicioCobro, hoy, abono.PeriodicidadCobro))
            {
                var cuota = abono.Cuotas.FirstOrDefault(c =>
                    c.Estado != EstadoCuota.Anulada && c.PeriodoInicio == inicio);

                decimal saldo;
                if (cuota == null)
                    saldo = montoBase;
                else if (cuota.Estado == EstadoCuota.Pagada || cuota.Saldo <= 0)
                    continue;
                else
                    saldo = cuota.Saldo;

                if (saldo <= 0) continue;

                saldoTotal += saldo;
                periodos++;
                primerImpago ??= inicio;
            }

            if (saldoTotal <= 0 || periodos == 0) continue;

            alertas.Add(new AlertaDeudorDto
            {
                AbonoId = abono.AbonoId,
                ClienteNombre = abono.Cliente?.Nombre ?? "Sin Nombre",
                ClienteTelefono = abono.Cliente?.Telefono,
                CocheraNumero = string.Join(", ", abono.Plazas.Where(p => p.Activo).Select(p => p.Cochera.Numero)),
                DiasAtraso = primerImpago.HasValue
                    ? Math.Max(0, (hoy.ToDateTime(TimeOnly.MinValue) - primerImpago.Value.ToDateTime(TimeOnly.MinValue)).Days)
                    : 0,
                PrecioAcordado = abono.PrecioAcordado,
                SaldoTotal = saldoTotal,
                PeriodosAdeudados = periodos,
            });
        }

        return alertas.OrderByDescending(a => a.DiasAtraso).ToList();
    }

    private static List<DistribucionVehiculoDto> GenerarDistribucionVehiculos(List<Abono> abonosActivos)
    {
        var vehiculos = abonosActivos.SelectMany(a => a.AbonoVehiculos.Select(av => av.Vehiculo)).ToList();
        var total = vehiculos.Count;
        return vehiculos.GroupBy(v => v.TipoVehiculo?.Nombre ?? "Otros")
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
            .GroupBy(p =>
            {
                var periodo = p.Detalles.Select(d => d.Cuota.PeriodoInicio).DefaultIfEmpty(DateOnly.FromDateTime(p.FechaHora)).Min();
                return new { periodo.Year, periodo.Month };
            })
            .Select(g => new { g.Key.Year, MesNum = g.Key.Month, Total = g.Sum(p => p.MontoTotal) })
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

    private static DetalleIngresosKpiDto CalcularDetalleIngresos(List<Abono> abonosActivos, decimal totalRecaudadoReal)
    {
        var totalEstimado = abonosActivos.Sum(a => a.PrecioAcordado ?? 0);
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
