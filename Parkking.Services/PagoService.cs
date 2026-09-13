using System.Globalization;
using Parkking.DTOs.Pagos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;
using Parkking.Services.Movimientos;

namespace Parkking.Services;

public class PagoService
{
    private readonly PagoRepository _repository;
    private readonly CajaMensualService _cajaService;
    private readonly ReciboService _reciboService;
    private readonly MetodoDePagoService _metodoDePagoService;
    private readonly AbonoPrecioService _precioService;
    private readonly IEstacionamientoContext _estacionamiento;

    public PagoService(
        PagoRepository repository,
        CajaMensualService cajaService,
        ReciboService reciboService,
        MetodoDePagoService metodoDePagoService,
        AbonoPrecioService precioService,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _cajaService = cajaService;
        _reciboService = reciboService;
        _metodoDePagoService = metodoDePagoService;
        _precioService = precioService;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public PagoConDetallesDto RegistrarPago(RegistrarPagoRequest request)
    {
        var abonoId = request.AbonoId;
        using var transaction = _repository.BeginTransaction();
        try
        {
            var abono = _repository.GetAbonoActivo(abonoId)
                ?? throw new Exception("Abono no encontrado o inactivo.");

            if (request.MetodoDePagoId <= 0)
                throw new Exception("Debe seleccionar un método de pago.");

            var metodo = _metodoDePagoService.RequireActivo(request.MetodoDePagoId);

            var lineas = ResolverLineasPago(request, abono);
            if (lineas.Count == 0)
                throw new Exception("El pago debe incluir al menos un detalle o Mes/Monto.");

            // No repetir la misma cuota/período en un mismo pago
            var periodos = lineas.Select(l => l.PeriodoInicio).ToList();
            if (periodos.Count != periodos.Distinct().Count())
                throw new Exception("No se puede aplicar más de un detalle a la misma cuota en un solo pago.");

            var cuotasAfectadas = new List<Cuota>();
            decimal totalAplicado = 0;

            foreach (var linea in lineas)
            {
                var cuota = AsegurarCuota(abono, linea.PeriodoInicio, linea.PeriodoFin, linea.Monto);
                var pagadoActual = _repository.GetMontoPagadoCuota(cuota.CuotaId);
                var saldo = Math.Max(0, cuota.Monto - pagadoActual);

                if (cuota.Estado == EstadoCuota.Pagada || saldo <= 0)
                    throw new Exception(
                        $"El período {FormatearPeriodo(cuota.PeriodoInicio, cuota.PeriodoFin, abono.PeriodicidadCobro)} ya está pagado. No se puede registrar otro cobro.");

                if (linea.Monto <= 0)
                    throw new Exception("Cada detalle debe tener monto mayor a cero.");

                if (linea.Monto > saldo)
                    throw new Exception(
                        $"El monto ({linea.Monto:0.##}) supera el saldo pendiente ({saldo:0.##}) del período {FormatearPeriodo(cuota.PeriodoInicio, cuota.PeriodoFin, abono.PeriodicidadCobro)}.");

                linea.Cuota = cuota;
                linea.MontoAplicado = linea.Monto;
                totalAplicado += linea.Monto;
                cuotasAfectadas.Add(cuota);
            }

            var recargo = request.Recargo ?? 0;
            var pago = new Pago
            {
                AbonoId = abonoId,
                EstacionamientoId = abono.EstacionamientoId,
                MontoTotal = totalAplicado + recargo,
                Recargo = request.Recargo,
                FechaHora = DateTime.UtcNow,
                Observacion = request.Observacion,
                MercadoPagoId = request.MercadoPagoId,
                MetodoDePagoId = metodo.MetodoDePagoId
            };
            _repository.AddPago(pago);
            _repository.SaveChanges();

            foreach (var linea in lineas)
            {
                _repository.AddDetallePago(new DetallePago
                {
                    EstacionamientoId = abono.EstacionamientoId,
                    PagoId = pago.PagoId,
                    CuotaId = linea.Cuota!.CuotaId,
                    Monto = linea.MontoAplicado
                });
            }
            _repository.SaveChanges();

            foreach (var cuotaId in cuotasAfectadas.Select(c => c.CuotaId).Distinct())
            {
                var cuota = _repository.GetCuotaById(cuotaId)!;
                var pagado = _repository.GetMontoPagadoCuota(cuotaId);
                cuota.RecalcularEstado(pagado);
            }
            _repository.SaveChanges();

            var cuotaPrincipal = cuotasAfectadas.OrderBy(c => c.PeriodoInicio).First();
            _cajaService.RegistrarMovimiento(new PagoAbonoStrategy(pago, abono, cuotaPrincipal));

            var periodosRecibo = cuotasAfectadas
                .OrderBy(c => c.PeriodoInicio)
                .Select(c => (
                    c.PeriodoInicio,
                    c.PeriodoFin,
                    FormatearPeriodo(c.PeriodoInicio, c.PeriodoFin, abono.PeriodicidadCobro)))
                .Distinct()
                .ToList();
            var recibo = _reciboService.GenerarDesdePago(pago, abono, periodosRecibo, metodo.Nombre);

            transaction.Commit();

            var dto = GetPagoConDetalles(pago.PagoId);
            dto.ReciboId = recibo.ReciboId;
            dto.ReciboNumero = recibo.NumeroFormateado;
            return dto;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>Cuotas / períodos a pagar del abono (con saldo &gt; 0).</summary>
    public List<CuotaPendienteDto> GetCuotasPendientes(int abonoId) =>
        GetCuotasTimeline(abonoId)
            .Where(c => !c.EsFuturo && c.Saldo > 0 && c.Estado != EstadoCuota.Pagada)
            .Select(c => (CuotaPendienteDto)c)
            .ToList();

    /// <summary>Todas las deudas operativas: períodos con saldo de abonos activos.</summary>
    public List<DeudaPendienteDto> GetPendientes()
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var result = new List<DeudaPendienteDto>();

        foreach (var abono in _repository.GetAbonosActivos())
        {
            var cocheras = string.Join(", ",
                abono.Plazas.Where(p => p.Activo).Select(p => p.Cochera?.Numero).Where(n => !string.IsNullOrEmpty(n)));
            var patentes = string.Join(", ",
                abono.AbonoVehiculos.Select(av => av.Vehiculo?.Patente).Where(p => !string.IsNullOrEmpty(p)));

            foreach (var (inicio, fin) in PeriodicidadHelper.EnumerarPeriodos(
                         abono.FechaInicioCobro, hoy, abono.PeriodicidadCobro))
            {
                var cuota = abono.Cuotas.FirstOrDefault(c =>
                    c.Estado != EstadoCuota.Anulada && c.PeriodoInicio == inicio);

                var montoBase = cuota?.Monto ?? ObtenerMontoBaseSafe(abono, inicio);

                decimal monto;
                decimal pagado;
                decimal saldo;
                EstadoCuota estado;
                int? cuotaId;

                if (cuota == null)
                {
                    if (montoBase <= 0) continue;
                    monto = montoBase;
                    pagado = 0;
                    saldo = montoBase;
                    estado = EstadoCuota.Pendiente;
                    cuotaId = null;
                }
                else if (cuota.Estado == EstadoCuota.Pagada || cuota.Saldo <= 0)
                {
                    continue;
                }
                else
                {
                    monto = cuota.Monto;
                    pagado = cuota.MontoPagado;
                    saldo = cuota.Saldo;
                    estado = cuota.Estado;
                    cuotaId = cuota.CuotaId;
                }

                result.Add(new DeudaPendienteDto
                {
                    AbonoId = abono.AbonoId,
                    CuotaId = cuotaId,
                    ClienteId = abono.ClienteId,
                    ClienteNombre = abono.Cliente?.Nombre ?? "Sin nombre",
                    ClienteTelefono = abono.Cliente?.Telefono,
                    CocherasLabel = cocheras,
                    PatentesLabel = string.IsNullOrWhiteSpace(patentes) ? null : patentes,
                    PeriodoInicio = inicio,
                    PeriodoFin = fin,
                    PeriodoLabel = FormatearPeriodo(inicio, fin, abono.PeriodicidadCobro),
                    Monto = monto,
                    MontoPagado = pagado,
                    Saldo = saldo,
                    Estado = estado,
                    DiasAtraso = Math.Max(0, (hoy.ToDateTime(TimeOnly.MinValue) - inicio.ToDateTime(TimeOnly.MinValue)).Days),
                });
            }
        }

        return result
            .OrderByDescending(d => d.DiasAtraso)
            .ThenBy(d => d.ClienteNombre)
            .ThenBy(d => d.PeriodoInicio)
            .ToList();
    }

    /// <summary>Timeline de períodos: pendientes, parciales, pagadas y el próximo futuro.</summary>
    public List<CuotaTimelineDto> GetCuotasTimeline(int abonoId)
    {
        var abono = _repository.GetAbonoConCuotas(abonoId)
            ?? throw new Exception("Abono no encontrado.");

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var result = new List<CuotaTimelineDto>();

        var ultimaCuotaPagada = abono.Cuotas
            .Where(c => c.Estado == EstadoCuota.Pagada)
            .Select(c => c.PeriodoInicio)
            .DefaultIfEmpty()
            .Max();

        var hasta = hoy;
        if (ultimaCuotaPagada != default && ultimaCuotaPagada > hasta)
            hasta = ultimaCuotaPagada;

        foreach (var (inicio, fin) in PeriodicidadHelper.EnumerarPeriodos(
                     abono.FechaInicioCobro, hasta, abono.PeriodicidadCobro))
        {
            result.Add(MapCuotaTimeline(abono, inicio, fin, esFuturo: false));
        }

        var ultimo = result.LastOrDefault();
        if (ultimo != null)
        {
            var (proxInicio, proxFin) = PeriodicidadHelper.SiguientePeriodo(
                ultimo.PeriodoInicio, ultimo.PeriodoFin, abono.PeriodicidadCobro);
            if (!result.Any(r => r.PeriodoInicio == proxInicio))
                result.Add(MapCuotaTimeline(abono, proxInicio, proxFin, esFuturo: true));
        }

        return result;
    }

    private CuotaTimelineDto MapCuotaTimeline(
        Abono abono, DateOnly inicio, DateOnly fin, bool esFuturo)
    {
        var cuota = abono.Cuotas.FirstOrDefault(c =>
            c.Estado != EstadoCuota.Anulada && c.PeriodoInicio == inicio);

        MontoPeriodoDto? precio = null;
        List<ComponenteTarifaDto> componentes;

        if (cuota?.Detalles is { Count: > 0 })
        {
            componentes = MapDetallesPersistidos(cuota.Detalles);
            precio = new MontoPeriodoDto
            {
                AbonoId = abono.AbonoId,
                Monto = cuota.Monto,
                EsPrecioAcordado = cuota.Detalles.Any(d => d.Tipo == TipoDetalleCuota.PrecioAcordado),
                ComponentesTarifa = componentes,
            };
        }
        else
        {
            precio = _precioService.TryResolverParaPeriodo(abono, inicio, fin);
            componentes = precio?.EsPrecioAcordado == false
                ? (precio.ComponentesTarifa ?? new List<ComponenteTarifaDto>())
                : new List<ComponenteTarifaDto>();
        }

        var montoBase = precio?.Monto ?? 0;
        var monto = cuota?.Monto ?? montoBase;
        var pagado = cuota?.MontoPagado ?? 0;
        var saldo = cuota != null ? cuota.Saldo : montoBase;
        var estado = cuota?.Estado ?? EstadoCuota.Pendiente;

        if (cuota == null && esFuturo)
            saldo = montoBase;

        var cobros = (cuota?.DetallesPago ?? Enumerable.Empty<DetallePago>())
            .OrderByDescending(d => d.Pago?.FechaHora ?? DateTime.MinValue)
            .Select(d => new CuotaCobroDto
            {
                DetallePagoId = d.DetallePagoId,
                PagoId = d.PagoId,
                Monto = d.Monto,
                FechaHora = d.Pago?.FechaHora ?? DateTime.MinValue,
                Observacion = d.Pago?.Observacion,
                RecargoPago = d.Pago?.Recargo,
                ReciboId = d.Pago?.Recibo?.ReciboId,
                ReciboNumero = d.Pago?.Recibo?.NumeroFormateado,
            })
            .ToList();

        var liquidacion = cuota?.Detalles is { Count: > 0 }
            ? MapDetalleCuotaDtos(cuota.Detalles)
            : new List<DetalleCuotaDto>();

        return new CuotaTimelineDto
        {
            CuotaId = cuota?.CuotaId,
            AbonoId = abono.AbonoId,
            PeriodoInicio = inicio,
            PeriodoFin = fin,
            PeriodoLabel = FormatearPeriodo(inicio, fin, abono.PeriodicidadCobro),
            Monto = monto,
            MontoPagado = pagado,
            Saldo = Math.Max(0, saldo),
            Estado = estado,
            EsFuturo = esFuturo && estado != EstadoCuota.Pagada && pagado <= 0,
            EsPrecioAcordado = precio?.EsPrecioAcordado ?? abono.PrecioAcordado.HasValue,
            ComponentesTarifa = componentes,
            DetallesLiquidacion = liquidacion,
            Cobros = cobros
        };
    }

    private static List<ComponenteTarifaDto> MapDetallesPersistidos(IEnumerable<DetalleCuota> detalles) =>
        detalles
            .Where(d => d.Tipo == TipoDetalleCuota.TarifaLista || d.Tipo == TipoDetalleCuota.PrecioAcordado)
            .Select(d => new ComponenteTarifaDto
            {
                TarifaMensualId = d.TarifaMensualId,
                VehiculoId = d.VehiculoId ?? 0,
                Patente = d.Patente ?? "",
                TipoVehiculoNombre = d.TipoVehiculoNombre,
                CocheraId = d.CocheraId ?? 0,
                CocheraNumero = d.CocheraNumero,
                CategoriaNombre = d.CategoriaNombre,
                Precio = d.Importe,
            })
            .ToList();

    private static List<DetalleCuotaDto> MapDetalleCuotaDtos(IEnumerable<DetalleCuota> detalles) =>
        detalles
            .OrderBy(d => d.DetalleCuotaId)
            .Select(d => new DetalleCuotaDto
            {
                DetalleCuotaId = d.DetalleCuotaId,
                CuotaId = d.CuotaId,
                TarifaMensualId = d.TarifaMensualId,
                Tipo = d.Tipo,
                TipoLabel = LabelTipoDetalle(d.Tipo),
                VehiculoId = d.VehiculoId,
                CocheraId = d.CocheraId,
                Patente = d.Patente,
                CocheraNumero = d.CocheraNumero,
                TipoVehiculoNombre = d.TipoVehiculoNombre,
                CategoriaNombre = d.CategoriaNombre,
                Descripcion = d.Descripcion,
                PrecioUnitario = d.PrecioUnitario,
                Cantidad = d.Cantidad,
                Importe = d.Importe,
            })
            .ToList();

    private static string LabelTipoDetalle(TipoDetalleCuota tipo) => tipo switch
    {
        TipoDetalleCuota.TarifaLista => "Tarifa de lista",
        TipoDetalleCuota.PrecioAcordado => "Precio acordado",
        TipoDetalleCuota.Prorrateo => "Prorrateo",
        TipoDetalleCuota.Recargo => "Recargo",
        TipoDetalleCuota.Descuento => "Descuento",
        TipoDetalleCuota.AjusteManual => "Ajuste",
        _ => tipo.ToString()
    };

    public List<DetalleCuotaDto> GetDetallesCuota(int cuotaId)
    {
        var cuota = _repository.GetCuotaById(cuotaId)
            ?? throw new Exception("Cuota no encontrada.");
        _repository.CargarDetallesLiquidacion(cuota);
        return MapDetalleCuotaDtos(cuota.Detalles ?? Enumerable.Empty<DetalleCuota>());
    }

    public MontoPeriodoDto GetMontoPeriodo(int abonoId, DateOnly? periodoInicio = null)
    {
        var abono = _repository.GetAbonoActivo(abonoId)
            ?? throw new Exception("Abono no encontrado.");
        var inicio = periodoInicio
            ?? PeriodicidadHelper.PeriodoQueContiene(
                DateOnly.FromDateTime(DateTime.UtcNow), abono.PeriodicidadCobro).Inicio;
        var fin = periodoInicio.HasValue
            ? PeriodicidadHelper.PeriodoQueContiene(inicio, abono.PeriodicidadCobro).Fin
            : PeriodicidadHelper.PeriodoQueContiene(
                DateOnly.FromDateTime(DateTime.UtcNow), abono.PeriodicidadCobro).Fin;
        return _precioService.ResolverParaPeriodo(abono, inicio, fin);
    }

    public PagoConDetallesDto GetPagoConDetalles(int pagoId)
    {
        var pago = _repository.GetPagoById(pagoId)
            ?? throw new Exception("Pago no encontrado.");
        return MapPagoConDetalles(pago, pago.Abono.PeriodicidadCobro);
    }

    public PagoSugeridoDto GetSugerido(int abonoId, DateOnly? mes)
    {
        var abono = _repository.GetAbonoActivo(abonoId) ?? throw new Exception("Abono no encontrado.");
        var datos = _repository.GetEstacionamiento(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        var fechaRef = mes ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var (periodoInicio, periodoFin) = PeriodicidadHelper.PeriodoQueContiene(fechaRef, abono.PeriodicidadCobro);

        var precio = _precioService.ResolverParaPeriodo(abono, periodoInicio, periodoFin);
        var tarifaBase = precio.Monto;
        var monto = tarifaBase;
        var aplicaProporcional = false;

        // Primera cuota del abono: prorrateo según umbral de DatosEstacionamiento (sin recargo)
        var montoProrrateado = datos.CalcularMontoPrimeraCuota(
            tarifaBase, abono.FechaInicio, periodoInicio, abono.PeriodicidadCobro);
        if (montoProrrateado < tarifaBase)
        {
            monto = montoProrrateado;
            aplicaProporcional = true;
        }

        var cuotaExistente = _repository.GetCuotaPorPeriodo(abonoId, periodoInicio);
        if (cuotaExistente != null)
        {
            _repository.RecargarDetallesCuota(cuotaExistente);
            if (cuotaExistente.Estado == EstadoCuota.Pagada || cuotaExistente.Saldo <= 0)
                throw new Exception(
                    $"El período {FormatearPeriodo(periodoInicio, periodoFin, abono.PeriodicidadCobro)} ya está pagado.");

            // Si ya hay cuota materializada, el saldo manda (ya incluye prorrateo al crear)
            monto = cuotaExistente.Saldo;
            aplicaProporcional = cuotaExistente.Monto < tarifaBase && cuotaExistente.MontoPagado == 0;
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var recargo = datos.CalcularRecargoSugerido(monto, periodoInicio, hoy, abono.PeriodicidadCobro);

        return new PagoSugeridoDto
        {
            Monto = monto,
            Recargo = recargo,
            AplicaProporcional = aplicaProporcional,
            EsPrecioAcordado = precio.EsPrecioAcordado,
            MesDate = periodoInicio,
            PeriodoInicio = periodoInicio,
            PeriodoFin = periodoFin,
            ComponentesTarifa = precio.ComponentesTarifa,
        };
    }

    public DeudaClienteDto GetDeuda(int clienteId)
    {
        var cliente = _repository.GetCliente(clienteId) ?? throw new Exception("Cliente no encontrado.");
        var abonos = _repository.GetAbonosActivosByCliente(clienteId);
        var mesesImpagos = new List<MesImpagoDto>();
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        foreach (var abono in abonos)
        {
            var plaza = abono.Plazas.FirstOrDefault(p => p.Activo);
            var vehiculo = abono.AbonoVehiculos.FirstOrDefault()?.Vehiculo;

            foreach (var (inicio, fin) in PeriodicidadHelper.EnumerarPeriodos(
                         abono.FechaInicioCobro, hoy, abono.PeriodicidadCobro))
            {
                var cuota = abono.Cuotas.FirstOrDefault(c =>
                    c.Estado != EstadoCuota.Anulada && c.PeriodoInicio == inicio);

                var montoBase = cuota?.Monto ?? ObtenerMontoBaseSafe(abono, inicio);

                decimal saldo;
                if (cuota == null)
                    saldo = montoBase;
                else if (cuota.Estado == EstadoCuota.Pagada)
                    continue;
                else
                    saldo = cuota.Saldo > 0 ? cuota.Saldo : montoBase;

                if (saldo <= 0)
                    continue;

                mesesImpagos.Add(new MesImpagoDto
                {
                    AbonoId = abono.AbonoId,
                    NumeroCochera = plaza?.Cochera?.Numero ?? "",
                    TipoVehiculo = vehiculo?.TipoVehiculo?.Nombre ?? "",
                    Mes = FormatearPeriodo(inicio, fin, abono.PeriodicidadCobro),
                    MesDate = inicio,
                    PeriodoInicio = inicio,
                    PeriodoFin = fin,
                    Monto = saldo,
                    Recargo = 0,
                    Total = saldo,
                    Saldo = saldo
                });
            }
        }

        return new DeudaClienteDto
        {
            ClienteId = clienteId,
            NombreCliente = cliente.Nombre,
            MesesImpagos = mesesImpagos
        };
    }

    public List<PagoConDetallesDto> GetByAbono(int abonoId)
    {
        var abono = _repository.GetAbonoConCuotas(abonoId);
        var periodicidad = abono?.PeriodicidadCobro ?? PeriodicidadCobro.Mensual;
        return _repository.GetByAbono(abonoId).Select(p => MapPagoConDetalles(p, periodicidad)).ToList();
    }

    public List<PagoConDetallesDto> GetByCliente(int clienteId) =>
        _repository.GetByCliente(clienteId)
            .Select(p => MapPagoConDetalles(p, p.Abono.PeriodicidadCobro))
            .ToList();

    public List<PagoConDetallesDto> GetAll(DateOnly? desde, DateOnly? hasta) =>
        _repository.GetAll(desde, hasta)
            .Select(p => MapPagoConDetalles(p, p.Abono.PeriodicidadCobro))
            .ToList();

    private List<LineaPagoInterna> ResolverLineasPago(RegistrarPagoRequest request, Abono abono)
    {
        if (request.Detalles is { Count: > 0 })
        {
            var lineas = new List<LineaPagoInterna>();
            foreach (var d in request.Detalles)
            {
                DateOnly inicio;
                DateOnly fin;

                if (d.CuotaId.HasValue)
                {
                    var cuota = _repository.GetCuotaById(d.CuotaId.Value)
                        ?? throw new Exception($"Cuota {d.CuotaId} no encontrada.");
                    if (cuota.AbonoId != abono.AbonoId)
                        throw new Exception("La cuota no pertenece a este abono.");
                    inicio = cuota.PeriodoInicio;
                    fin = cuota.PeriodoFin;
                }
                else if (!string.IsNullOrWhiteSpace(d.PeriodoInicio) &&
                         DateOnly.TryParseExact(d.PeriodoInicio, "yyyy-MM-dd", out var fechaRef))
                {
                    (inicio, fin) = PeriodicidadHelper.PeriodoQueContiene(fechaRef, abono.PeriodicidadCobro);
                }
                else
                    throw new Exception("Cada detalle requiere CuotaId o PeriodoInicio (yyyy-MM-dd).");

                var (inicioCobro, _) = PeriodicidadHelper.PeriodoQueContiene(abono.FechaInicioCobro, abono.PeriodicidadCobro);
                if (inicio < inicioCobro)
                    throw new Exception("No se puede registrar un pago anterior al inicio de cobros.");

                lineas.Add(new LineaPagoInterna
                {
                    PeriodoInicio = inicio,
                    PeriodoFin = fin,
                    Monto = d.Monto
                });
            }
            return lineas;
        }

        // Compat: Mes + Monto
        if (string.IsNullOrWhiteSpace(request.Mes))
            return new List<LineaPagoInterna>();

        if (!DateOnly.TryParseExact(request.Mes, "yyyy-MM-dd", out var mesRef))
            throw new Exception("El formato de fecha debe ser yyyy-MM-dd.");

        var (pInicio, pFin) = PeriodicidadHelper.PeriodoQueContiene(mesRef, abono.PeriodicidadCobro);
        var (limiteCobro, _) = PeriodicidadHelper.PeriodoQueContiene(abono.FechaInicioCobro, abono.PeriodicidadCobro);
        if (pInicio < limiteCobro)
            throw new Exception("No se puede registrar un pago anterior al inicio de los cobros del abono.");

        return new List<LineaPagoInterna>
        {
            new() { PeriodoInicio = pInicio, PeriodoFin = pFin, Monto = request.Monto }
        };
    }

    private Cuota AsegurarCuota(Abono abono, DateOnly periodoInicio, DateOnly periodoFin, decimal montoReferencia)
    {
        var cuota = _repository.GetCuotaPorPeriodo(abono.AbonoId, periodoInicio);
        if (cuota != null)
            return cuota;

        var precio = _precioService.TryResolverParaPeriodo(abono, periodoInicio, periodoFin);
        var montoCuota = precio?.Monto ?? 0;
        if (montoCuota <= 0)
            montoCuota = montoReferencia;

        var detalles = precio != null
            ? _precioService.CrearDetallesLiquidacion(abono, precio, montoCuota)
            : new List<DetalleCuota>
            {
                new()
                {
                    EstacionamientoId = abono.EstacionamientoId,
                    Tipo = TipoDetalleCuota.AjusteManual,
                    Descripcion = "Monto informado al cobrar",
                    PrecioUnitario = montoCuota,
                    Cantidad = 1,
                    Importe = montoCuota,
                    CreadoEn = DateTime.UtcNow,
                }
            };

        cuota = new Cuota
        {
            AbonoId = abono.AbonoId,
            EstacionamientoId = abono.EstacionamientoId,
            PeriodoInicio = periodoInicio,
            PeriodoFin = periodoFin,
            Monto = montoCuota,
            Estado = EstadoCuota.Pendiente,
            Detalles = detalles,
        };
        _repository.AddCuota(cuota);
        _repository.SaveChanges();
        return cuota;
    }

    private PagoConDetallesDto MapPagoConDetalles(Pago p, PeriodicidadCobro periodicidad)
    {
        var detalles = p.Detalles
            .OrderBy(d => d.Cuota.PeriodoInicio)
            .Select(d => new DetallePagoDto
            {
                DetallePagoId = d.DetallePagoId,
                PagoId = d.PagoId,
                CuotaId = d.CuotaId,
                Monto = d.Monto,
                PeriodoInicio = d.Cuota.PeriodoInicio,
                PeriodoFin = d.Cuota.PeriodoFin,
                PeriodoLabel = FormatearPeriodo(d.Cuota.PeriodoInicio, d.Cuota.PeriodoFin, periodicidad),
                EstadoCuota = d.Cuota.Estado,
                MontoCuota = d.Cuota.Monto,
                SaldoCuota = d.Cuota.Saldo
            })
            .ToList();

        var primero = detalles.FirstOrDefault();
        var plaza = p.Abono?.Plazas?.FirstOrDefault(pl => pl.Activo)
            ?? p.Abono?.Plazas?.FirstOrDefault();
        return new PagoConDetallesDto
        {
            PagoId = p.PagoId,
            AbonoId = p.AbonoId,
            Mes = primero?.PeriodoInicio ?? DateOnly.FromDateTime(p.FechaHora),
            FechaHoraCarga = p.FechaHora,
            Monto = p.MontoTotal - (p.Recargo ?? 0),
            Recargo = p.Recargo,
            Observacion = p.Observacion,
            MercadoPagoId = p.MercadoPagoId,
            MetodoDePagoId = p.MetodoDePagoId,
            MetodoDePagoNombre = p.MetodoDePago?.Nombre,
            CuotaId = primero?.CuotaId,
            PeriodoInicio = primero?.PeriodoInicio,
            PeriodoFin = primero?.PeriodoFin,
            ClienteNombre = p.Abono?.Cliente?.Nombre,
            NumeroCochera = plaza?.Cochera?.Numero,
            ReciboId = p.Recibo?.ReciboId,
            ReciboNumero = p.Recibo?.NumeroFormateado,
            Detalles = detalles
        };
    }

    private decimal ObtenerMontoBaseSafe(Abono abono, DateOnly periodoInicio)
    {
        var fin = PeriodicidadHelper.PeriodoQueContiene(periodoInicio, abono.PeriodicidadCobro).Fin;
        return _precioService.TryResolverParaPeriodo(abono, periodoInicio, fin)?.Monto ?? 0;
    }

    private static string FormatearPeriodo(DateOnly inicio, DateOnly fin, PeriodicidadCobro p) =>
        p switch
        {
            PeriodicidadCobro.Quincenal => inicio.Day <= 15
                ? $"1ª quincena {inicio.ToString("MMMM yyyy", new CultureInfo("es-AR"))}"
                : $"2ª quincena {inicio.ToString("MMMM yyyy", new CultureInfo("es-AR"))}",
            PeriodicidadCobro.Mensual => inicio.ToString("MMMM yyyy", new CultureInfo("es-AR")),
            _ => $"{inicio:dd/MM/yyyy} – {fin:dd/MM/yyyy}"
        };

    private sealed class LineaPagoInterna
    {
        public DateOnly PeriodoInicio { get; set; }
        public DateOnly PeriodoFin { get; set; }
        public decimal Monto { get; set; }
        public decimal MontoAplicado { get; set; }
        public Cuota? Cuota { get; set; }
    }
}
