using Parkking.DTOs.Pagos;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;

namespace Parkking.Services;

/// <summary>
/// Resuelve el monto base de un período y arma las líneas de liquidación (DetalleCuota).
/// </summary>
public class AbonoPrecioService
{
    private readonly TarifaMensualRepository _tarifas;

    public AbonoPrecioService(TarifaMensualRepository tarifas) => _tarifas = tarifas;

    /// <summary>
    /// Precio del período: tarifa de lista vigente al liquidar.
    /// Período actual o futuro → tarifa de ahora; período ya cerrado → tarifa al inicio.
    /// </summary>
    public MontoPeriodoDto ResolverParaPeriodo(
        Abono abono,
        DateOnly periodoInicio,
        DateOnly? periodoFin = null) =>
        Resolver(abono, ReferenciaDePeriodo(periodoInicio, periodoFin));

    /// <summary>Precio con la tarifa vigente “ahora” (alta / preview).</summary>
    public MontoPeriodoDto ResolverVigente(Abono abono) =>
        Resolver(abono, DateTime.UtcNow);

    public MontoPeriodoDto Resolver(Abono abono, DateTime? referencia = null)
    {
        if (abono.PrecioAcordado.HasValue && abono.PrecioAcordado.Value > 0)
        {
            return new MontoPeriodoDto
            {
                AbonoId = abono.AbonoId,
                Monto = abono.PrecioAcordado.Value,
                EsPrecioAcordado = true,
            };
        }

        var vehiculos = abono.AbonoVehiculos?.ToList() ?? new List<AbonoVehiculo>();
        if (vehiculos.Any(v => v.Modalidad == ModalidadVehiculoAbono.Flexible))
        {
            throw new Exception(
                "Los abonos con vehículos flexibles requieren precio acordado: " +
                "aún no se puede calcular tarifa de lista sin saber qué auto ocupa cada cochera.");
        }

        if (vehiculos.Count == 0)
        {
            throw new Exception(
                "El abono no tiene precio acordado ni vehículos fijos para calcular tarifa de lista.");
        }

        var refDate = referencia ?? DateTime.UtcNow;
        var componentes = new List<ComponenteTarifaDto>();
        decimal total = 0;

        foreach (var av in vehiculos.OrderBy(v => v.AbonoVehiculoId))
        {
            if (av.Modalidad != ModalidadVehiculoAbono.Fijo)
                continue;

            var vehiculo = av.Vehiculo
                ?? throw new Exception("Vehículo del abono sin datos cargados.");
            var plaza = av.AbonoPlaza
                ?? abono.Plazas?.FirstOrDefault(p => p.AbonoPlazaId == av.AbonoPlazaId);
            var cochera = plaza?.Cochera
                ?? throw new Exception(
                    $"El vehículo {vehiculo.Patente} es fijo pero no tiene plaza asignada.");

            var categoriaId = cochera.CategoriaCocheraId;
            var tarifa = _tarifas.GetVigente(
                vehiculo.TipoVehiculoId,
                categoriaId,
                abono.PeriodicidadCobro,
                abono.EstacionamientoId,
                refDate);

            if (tarifa == null)
            {
                var tipoNombre = vehiculo.TipoVehiculo?.Nombre ?? $"tipo {vehiculo.TipoVehiculoId}";
                var catNombre = cochera.CategoriaCochera?.Nombre ?? $"categoría {categoriaId}";
                throw new Exception(
                    $"No hay tarifa vigente para {vehiculo.Patente} ({tipoNombre} / {catNombre} / {abono.PeriodicidadCobro}).");
            }

            componentes.Add(new ComponenteTarifaDto
            {
                TarifaMensualId = tarifa.TarifaMensualId,
                VehiculoId = vehiculo.VehiculoId,
                Patente = vehiculo.Patente,
                TipoVehiculoId = vehiculo.TipoVehiculoId,
                TipoVehiculoNombre = vehiculo.TipoVehiculo?.Nombre,
                CocheraId = cochera.CocheraId,
                CocheraNumero = cochera.Numero,
                CategoriaCocheraId = categoriaId,
                CategoriaNombre = cochera.CategoriaCochera?.Nombre,
                PeriodicidadCobro = abono.PeriodicidadCobro,
                Precio = tarifa.Precio,
            });
            total += tarifa.Precio;
        }

        if (componentes.Count == 0)
            throw new Exception("No se pudo armar el monto: no hay vehículos fijos con plaza.");

        return new MontoPeriodoDto
        {
            AbonoId = abono.AbonoId,
            Monto = total,
            EsPrecioAcordado = false,
            ComponentesTarifa = componentes,
        };
    }

    public MontoPeriodoDto? TryResolverParaPeriodo(
        Abono abono,
        DateOnly periodoInicio,
        DateOnly? periodoFin = null)
    {
        try { return ResolverParaPeriodo(abono, periodoInicio, periodoFin); }
        catch { return null; }
    }

    public MontoPeriodoDto? TryResolver(Abono abono, DateTime? referencia = null)
    {
        try { return Resolver(abono, referencia); }
        catch { return null; }
    }

    /// <summary>
    /// Arma las líneas a persistir en la cuota.
    /// Si montoFinal ≠ suma base (ej. prorrateo), agrega una línea Tipo=Prorrateo con la diferencia.
    /// </summary>
    public List<DetalleCuota> CrearDetallesLiquidacion(
        Abono abono,
        MontoPeriodoDto precio,
        decimal? montoFinal = null)
    {
        var ahora = DateTime.UtcNow;
        var detalles = new List<DetalleCuota>();

        if (precio.EsPrecioAcordado)
        {
            var importe = montoFinal ?? precio.Monto;
            detalles.Add(new DetalleCuota
            {
                EstacionamientoId = abono.EstacionamientoId,
                Tipo = TipoDetalleCuota.PrecioAcordado,
                Descripcion = "Precio acordado",
                PrecioUnitario = importe,
                Cantidad = 1,
                Importe = importe,
                CreadoEn = ahora,
            });
            return detalles;
        }

        foreach (var c in precio.ComponentesTarifa)
        {
            detalles.Add(new DetalleCuota
            {
                EstacionamientoId = abono.EstacionamientoId,
                TarifaMensualId = c.TarifaMensualId,
                Tipo = TipoDetalleCuota.TarifaLista,
                VehiculoId = c.VehiculoId,
                CocheraId = c.CocheraId > 0 ? c.CocheraId : null,
                Patente = c.Patente,
                CocheraNumero = c.CocheraNumero,
                TipoVehiculoNombre = c.TipoVehiculoNombre,
                CategoriaNombre = c.CategoriaNombre,
                Descripcion = $"{c.Patente} · {c.TipoVehiculoNombre ?? "vehículo"} · {c.CategoriaNombre ?? "categoría"}",
                PrecioUnitario = c.Precio,
                Cantidad = 1,
                Importe = c.Precio,
                CreadoEn = ahora,
            });
        }

        var suma = detalles.Sum(d => d.Importe);
        var final = montoFinal ?? suma;
        var diferencia = final - suma;
        if (Math.Abs(diferencia) >= 0.01m)
        {
            detalles.Add(new DetalleCuota
            {
                EstacionamientoId = abono.EstacionamientoId,
                Tipo = TipoDetalleCuota.Prorrateo,
                Descripcion = diferencia < 0 ? "Ajuste por prorrateo" : "Ajuste de monto",
                PrecioUnitario = diferencia,
                Cantidad = 1,
                Importe = diferencia,
                CreadoEn = ahora,
            });
        }

        return detalles;
    }

    /// <summary>
    /// Referencia temporal para elegir la fila de tarifa (historial por FechaHoraActualizacion).
    /// Si el período aún no terminó (incluye el mes/quincena en curso), usa UtcNow para
    /// coincidir con “tarifa vigente” del alta/preview. Si ya cerró, congela al inicio.
    /// </summary>
    public static DateTime ReferenciaDePeriodo(DateOnly periodoInicio, DateOnly? periodoFin = null)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var fin = periodoFin ?? periodoInicio;
        if (fin < hoy)
            return DateTime.SpecifyKind(periodoInicio.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        return DateTime.UtcNow;
    }

    public static bool TieneVehiculoFlexible(Abono abono) =>
        abono.AbonoVehiculos?.Any(v => v.Modalidad == ModalidadVehiculoAbono.Flexible) == true;
}
