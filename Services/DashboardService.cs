using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.DTOs.Dashboard;
using System.Globalization;

namespace Parkking_backend.Services
{
    public class DashboardService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public DashboardService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public DashboardAbonosDto GetSummary()
        {
            var estacionamientoId = _estacionamiento.EstacionamientoId;

            // 1. CARGA DE DATOS BASE DESDE LA BD
            var cocheras = _context.Cocheras
                .Include(c => c.CategoriaCochera)
                .Include(c => c.Abonos.Where(a => a.Activo))
                    .ThenInclude(a => a.Cliente)
                .Include(c => c.Abonos.Where(a => a.Activo))
                    .ThenInclude(a => a.TipoVehiculo)
                .Include(c => c.Abonos.Where(a => a.Activo))
                    .ThenInclude(a => a.PagosMensuales)
                .Where(c => c.EstacionamientoId == estacionamientoId && c.Activo)
                .ToList();

            var abonosActivos = cocheras.SelectMany(c => c.Abonos).ToList();

            Func<int, int, decimal> obtenerTarifaVigenteActual = (tipoVehiculoId, categoriaId) =>
            {
                return _context.TarifasMensuales
                    .Where(t => t.TipoVehiculoId == tipoVehiculoId
                             && t.CategoriaCocheraId == categoriaId
                             && t.EstacionamientoId == estacionamientoId
                             && t.FechaHoraActualizacion <= DateTime.Now)
                    .OrderByDescending(t => t.FechaHoraActualizacion)
                    .Select(t => t.Precio)
                    .FirstOrDefault();
            };

            // 2. PROCESAMIENTO MODULARIZADO (Armado del JSON unificado)
            var historicoIngresos = GenerarHistoricoIngresos(estacionamientoId);

            // Obtenemos la recaudación real de este mes desde el histórico generado
            decimal totalRecaudadoRealMes = historicoIngresos.LastOrDefault()?.Monto ?? 0;

            var dashboardDto = new DashboardAbonosDto
            {
                // Módulo 1: Ocupación
                CocherasTotales = cocheras.Count,
                CocherasOcupadas = cocheras.Count(c => c.Abonos.Any(a => a.Activo)),
                PorcentajeOcupacion = CalcularPorcentajeOcupacion(cocheras),

                // Módulo 2: Alertas e info de abonos
                CantidadAbonosActivos = abonosActivos.Count,
                EstadoCocheras = GenerarMapaCocheras(cocheras),
                AlertasDeudores = GenerarAlertasDeudores(abonosActivos, cocheras),
                DistribucionVehiculos = GenerarDistribucionVehiculos(abonosActivos),

                // Módulo 3: Gráfico Histórico
                GraficoIngresos = historicoIngresos,

                // NUEVO MÓDULO 4: Finanzas Detalladas (Métricas combinadas para la Card)
                DetalleIngresos = CalcularDetalleIngresos(abonosActivos, totalRecaudadoRealMes, obtenerTarifaVigenteActual)
            };

            return dashboardDto;
        }

        private double CalcularPorcentajeOcupacion(List<Cochera> cocheras)
        {
            int totales = cocheras.Count;
            int ocupadas = cocheras.Count(c => c.Abonos.Any(a => a.Activo));
            return totales > 0 ? Math.Round(((double)ocupadas / totales) * 100, 1) : 0;
        }

        private List<CocheraStatusDto> GenerarMapaCocheras(List<Cochera> cocheras)
        {
            return cocheras.Select(c => {
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
        }

        private List<AlertaDeudorDto> GenerarAlertasDeudores(List<AbonoCochera> abonosActivos, List<Cochera> cocheras)
        {
            var alertas = new List<AlertaDeudorDto>();
            foreach (var abono in abonosActivos)
            {
                if (!abono.TieneDeuda()) continue;

                alertas.Add(new AlertaDeudorDto
                {
                    AbonoCocheraId = abono.AbonoCocheraId,
                    ClienteNombre = abono.Cliente?.Nombre ?? "Sin Nombre",
                    ClienteTelefono = abono.Cliente?.Telefono,
                    CocheraNumero = cocheras.FirstOrDefault(c => c.CocheraId == abono.CocheraId)?.Numero ?? "",
                    DiasAtraso = abono.ObtenerDiasAtraso(),
                    PrecioAcordado = abono.PrecioAcordado
                });
            }
            return alertas.OrderByDescending(a => a.DiasAtraso).ToList();
        }

        private List<DistribucionVehiculoDto> GenerarDistribucionVehiculos(List<AbonoCochera> abonosActivos)
        {
            var totalVehiculos = abonosActivos.Count;
            return abonosActivos
                .GroupBy(a => a.TipoVehiculo.Nombre ?? "Otros")
                .Select(g => new DistribucionVehiculoDto
                {
                    TipoVehiculo = g.Key,
                    Cantidad = g.Count(),
                    Porcentaje = totalVehiculos > 0 ? Math.Round(((double)g.Count() / totalVehiculos) * 100, 1) : 0
                }).ToList();
        }

        private List<HistoricoIngresosDto> GenerarHistoricoIngresos(int estacionamientoId)
        {
            var fechaLimite = DateOnly.FromDateTime(DateTime.Today.AddMonths(-5));
            var primerDiaLimite = new DateOnly(fechaLimite.Year, fechaLimite.Month, 1);

            var pagosReales = _context.PagosMensuales
                .Include(p => p.AbonoCochera)
                    .ThenInclude(a => a.Cochera)
                .Where(p => p.AbonoCochera.Cochera.EstacionamientoId == estacionamientoId && p.Mes >= primerDiaLimite)
                .ToList();

            var ingresosAgrupados = pagosReales
                .GroupBy(p => new { p.Mes.Year, p.Mes.Month })
                .Select(g => new {
                    Año = g.Key.Year,
                    MesNum = g.Key.Month,
                    Total = g.Sum(p => p.Monto + (p.Recargo ?? 0))
                }).ToList();

            var graficoIngresos = new List<HistoricoIngresosDto>();

            for (int i = 5; i >= 0; i--)
            {
                var fechaMes = DateTime.Today.AddMonths(-i);
                var año = fechaMes.Year;
                var mesNum = fechaMes.Month;

                var nombreMes = fechaMes.ToString("MMM", new CultureInfo("es-ES"));
                nombreMes = char.ToUpper(nombreMes[0]) + nombreMes.Substring(1).Replace(".", "");

                var totalMonto = ingresosAgrupados
                    .FirstOrDefault(x => x.Año == año && x.MesNum == mesNum)?.Total ?? 0;

                graficoIngresos.Add(new HistoricoIngresosDto
                {
                    Mes = nombreMes,
                    Monto = totalMonto
                });
            }

            return graficoIngresos;
        }

        private DetalleIngresosKpiDto CalcularDetalleIngresos(
            List<AbonoCochera> abonosActivos,
            decimal totalRecaudadoReal,
            Func<int, int, decimal> obtenerTarifaListaFallback)
        {
            // 1. Calculamos la proyección teórica usando el nuevo método estratégico de la entidad
            decimal totalEstimadoProyeccion = abonosActivos.Sum(a => a.CalcularMontoMensual(obtenerTarifaListaFallback));
            double diferenciaPorcentaje = 0;
            if (totalEstimadoProyeccion > 0)
            {
                decimal diferenciaAbsoluta = totalRecaudadoReal - totalEstimadoProyeccion;
                diferenciaPorcentaje = Math.Round((double)(diferenciaAbsoluta / totalEstimadoProyeccion) * 100, 1);
            }

            return new DetalleIngresosKpiDto
            {
                TotalRecaudadoMensualReal = totalRecaudadoReal,
                TotalEstimadoProyeccion = totalEstimadoProyeccion,
                DiferenciaPorcentaje = diferenciaPorcentaje // Ej: -50.0 (falta el 50%) o +10.5 (superó el estimado)
            };
        }
    }
}
