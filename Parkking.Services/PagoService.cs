using System.Globalization;
using Parkking.DTOs.Pagos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Cobro;
using Parkking.Models.Finanzas;
using Parkking.Models.Enums;
using Parkking.Repositories;
using Parkking.Services.Mensajeria;

namespace Parkking.Services;

public class PagoService
{
    private readonly PagoRepository _repository;
    private readonly MovimientoService _movimientos;
    private readonly ReciboService _reciboService;
    private readonly MetodoDePagoService _metodoDePagoService;
    private readonly AbonoPrecioService _precioService;
    private readonly MensajeService _mensajes;
    private readonly IEstacionamientoContext _estacionamiento;

    public PagoService(
        PagoRepository repository,
        MovimientoService movimientos,
        ReciboService reciboService,
        MetodoDePagoService metodoDePagoService,
        AbonoPrecioService precioService,
        MensajeService mensajes,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _movimientos = movimientos;
        _reciboService = reciboService;
        _metodoDePagoService = metodoDePagoService;
        _precioService = precioService;
        _mensajes = mensajes;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public async Task<PagoConDetallesDto> RegistrarPagoAsync(
        RegistrarPagoRequest request,
        CancellationToken ct = default)
    {
        var abonoId = request.AbonoId;
        using var transaction = _repository.BeginTransaction();
        Recibo? recibo;
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

            var movimiento = new Movimiento
            {
                EstacionamientoId = abono.EstacionamientoId,
                ClienteId = abono.ClienteId,
                PagoId = pago.PagoId,
                AbonoId = abono.AbonoId,
                Importe = pago.MontoTotal,
                Tipo = TipoMovimiento.Ingreso,
                Concepto = "Cobro de abono",
                FechaHora = pago.FechaHora,
                UsuarioId = _estacionamiento.GetUsuarioActual().Id
            };
            _movimientos.Registrar(movimiento, Array.Empty<int>(), "Alta de movimiento por cobro");

            var periodosRecibo = cuotasAfectadas
                .OrderBy(c => c.PeriodoInicio)
                .Select(c => (
                    c.PeriodoInicio,
                    c.PeriodoFin,
                    FormatearPeriodo(c.PeriodoInicio, c.PeriodoFin, abono.PeriodicidadCobro)))
                .Distinct()
                .ToList();
            recibo = _reciboService.GenerarDesdePago(pago, abono, periodosRecibo, metodo.Nombre);

            transaction.Commit();

            var dto = GetPagoConDetalles(pago.PagoId);
            dto.ReciboId = recibo.ReciboId;
            dto.ReciboNumero = recibo.NumeroFormateado;

            await IntentarEnviarReciboPorEmailAsync(dto, recibo.ReciboId, ct);
            return dto;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    /// <summary>Compat sync para callers internos (si los hubiera).</summary>
    public PagoConDetallesDto RegistrarPago(RegistrarPagoRequest request) =>
        RegistrarPagoAsync(request).GetAwaiter().GetResult();

    private async Task IntentarEnviarReciboPorEmailAsync(
        PagoConDetallesDto dto,
        int reciboId,
        CancellationToken ct)
    {
        var datos = _repository.GetEstacionamiento(TenantId);
        if (datos is null || !datos.EnviarReciboPorEmail)
            return;

        dto.ReciboEmailIntentado = true;
        try
        {
            var msg = await _mensajes.EnviarReciboAsync(reciboId, emailOverride: null, ct);
            dto.ReciboEmailEnviado = true;
            dto.ReciboEmailSimulado = msg.Simulado;
            dto.ReciboEmailDestinatario = msg.Destinatario;
        }
        catch (Exception ex)
        {
            // El cobro ya está confirmado: no fallar el pago por el mail.
            dto.ReciboEmailEnviado = false;
            dto.ReciboEmailError = ex.Message;
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

            var strategy = PeriodicidadStrategyFactory.For(abono.PeriodicidadCobro);
            var ancla = Ancla(abono);
            foreach (var periodo in strategy.Enumerar(abono.FechaInicioCobro, hoy, ancla))
            {
                var inicio = periodo.Inicio;
                var fin = periodo.Fin;
                var cuota = BuscarCuotaDelPeriodo(abono, inicio, fin);

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

        // Incluir al menos hasta la 1ª cuota / inicio de cobro (p.ej. quincenal con
        // OmitirMesEntrante que arranca en la quincena siguiente aún futura).
        var hasta = hoy;
        if (abono.FechaInicioCobro > hasta)
            hasta = abono.FechaInicioCobro;

        var maxCuotaInicio = abono.Cuotas
            .Where(c => c.Estado != EstadoCuota.Anulada)
            .Select(c => c.PeriodoInicio)
            .DefaultIfEmpty()
            .Max();
        if (maxCuotaInicio != default && maxCuotaInicio > hasta)
            hasta = maxCuotaInicio;

        var ultimaCuotaPagada = abono.Cuotas
            .Where(c => c.Estado == EstadoCuota.Pagada)
            .Select(c => c.PeriodoInicio)
            .DefaultIfEmpty()
            .Max();
        if (ultimaCuotaPagada != default && ultimaCuotaPagada > hasta)
            hasta = ultimaCuotaPagada;

        var strategy = PeriodicidadStrategyFactory.For(abono.PeriodicidadCobro);
        var ancla = Ancla(abono);
        foreach (var periodo in strategy.Enumerar(abono.FechaInicioCobro, hasta, ancla))
        {
            var esFuturo = periodo.Inicio > hoy;
            result.Add(MapCuotaTimeline(abono, periodo.Inicio, periodo.Fin, esFuturo));
        }

        var ultimo = result.LastOrDefault();
        if (ultimo != null)
        {
            var prox = strategy.SiguienteCiclo(
                new PeriodoCobro(ultimo.PeriodoInicio, ultimo.PeriodoFin), ancla);
            if (!result.Any(r => r.PeriodoInicio == prox.Inicio))
                result.Add(MapCuotaTimeline(abono, prox.Inicio, prox.Fin, esFuturo: prox.Inicio > hoy));
        }

        return result;
    }

    private CuotaTimelineDto MapCuotaTimeline(
        Abono abono, DateOnly inicio, DateOnly fin, bool esFuturo)
    {
        var cuota = BuscarCuotaDelPeriodo(abono, inicio, fin);

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
        var ancla = Ancla(abono);
        var refFecha = periodoInicio ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var periodo = ResolvePeriodoParaFecha(abono, refFecha);
        return _precioService.ResolverParaPeriodo(abono, periodo.Inicio, periodo.Fin);
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
        if (fechaRef < abono.FechaInicioCobro)
            throw new Exception("No se puede sugerir un período anterior al inicio de cobros.");

        var strategy = PeriodicidadStrategyFactory.For(abono.PeriodicidadCobro);
        var ancla = Ancla(abono);
        var cicloNatural = strategy.CicloQueContiene(
            fechaRef < abono.FechaInicio ? abono.FechaInicio : fechaRef, ancla);
        var precio = _precioService.ResolverParaPeriodo(abono, cicloNatural.Inicio, cicloNatural.Fin);
        var tarifaBase = precio.Monto;

        var primer = strategy.ResolverPrimerPeriodo(
            abono.FechaInicio, abono.PoliticaPrimerPeriodo, tarifaBase, ancla);

        PeriodoCobro periodo;
        decimal monto;
        var aplicaProporcional = false;

        if (primer.Periodo.Contiene(fechaRef) || fechaRef == abono.FechaInicioCobro)
        {
            periodo = new PeriodoCobro(abono.FechaInicioCobro, primer.Periodo.Fin);
            monto = primer.Monto;
            aplicaProporcional = primer.FueProrrateado;
        }
        else
        {
            periodo = strategy.CicloQueContiene(fechaRef, ancla);
            monto = tarifaBase;
        }

        var cuotaExistente = _repository.GetCuotaPorPeriodo(abonoId, periodo.Inicio);
        if (cuotaExistente != null)
        {
            _repository.RecargarDetallesCuota(cuotaExistente);
            if (cuotaExistente.Estado == EstadoCuota.Pagada || cuotaExistente.Saldo <= 0)
                throw new Exception(
                    $"El período {FormatearPeriodo(periodo.Inicio, periodo.Fin, abono.PeriodicidadCobro)} ya está pagado.");

            monto = cuotaExistente.Saldo;
            aplicaProporcional = cuotaExistente.Monto < tarifaBase && cuotaExistente.MontoPagado == 0;
            periodo = new PeriodoCobro(cuotaExistente.PeriodoInicio, cuotaExistente.PeriodoFin);
        }

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var recargo = datos.CalcularRecargoSugerido(monto, periodo.Inicio, hoy, abono.PeriodicidadCobro);

        return new PagoSugeridoDto
        {
            Monto = monto,
            Recargo = recargo,
            AplicaProporcional = aplicaProporcional,
            EsPrecioAcordado = precio.EsPrecioAcordado,
            MesDate = periodo.Inicio,
            PeriodoInicio = periodo.Inicio,
            PeriodoFin = periodo.Fin,
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

            var strategy = PeriodicidadStrategyFactory.For(abono.PeriodicidadCobro);
            var ancla = Ancla(abono);
            foreach (var periodo in strategy.Enumerar(abono.FechaInicioCobro, hoy, ancla))
            {
                var inicio = periodo.Inicio;
                var fin = periodo.Fin;
                var cuota = BuscarCuotaDelPeriodo(abono, inicio, fin);

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
                    var periodoResuelto = ResolvePeriodoParaFecha(abono, fechaRef);
                    inicio = periodoResuelto.Inicio;
                    fin = periodoResuelto.Fin;
                }
                else
                    throw new Exception("Cada detalle requiere CuotaId o PeriodoInicio (yyyy-MM-dd).");

                if (inicio < abono.FechaInicioCobro)
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

        var p = ResolvePeriodoParaFecha(abono, mesRef);
        if (p.Inicio < abono.FechaInicioCobro)
            throw new Exception("No se puede registrar un pago anterior al inicio de los cobros del abono.");

        return new List<LineaPagoInterna>
        {
            new() { PeriodoInicio = p.Inicio, PeriodoFin = p.Fin, Monto = request.Monto }
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
        var periodo = ResolvePeriodoParaFecha(abono, periodoInicio);
        return _precioService.TryResolverParaPeriodo(abono, periodo.Inicio, periodo.Fin)?.Monto ?? 0;
    }

    /// <summary>
    /// Busca la cuota del período: match exacto por inicio, o solapamiento
    /// (1ª cuota irregular / datos legacy).
    /// </summary>
    private static Cuota? BuscarCuotaDelPeriodo(Abono abono, DateOnly inicio, DateOnly fin)
    {
        var vigentes = abono.Cuotas.Where(c => c.Estado != EstadoCuota.Anulada);

        var exacta = vigentes.FirstOrDefault(c => c.PeriodoInicio == inicio);
        if (exacta != null)
            return exacta;

        return vigentes.FirstOrDefault(c =>
            c.PeriodoInicio <= fin && c.PeriodoFin >= inicio);
    }

    private static DateOnly Ancla(Abono abono) =>
        PeriodicidadStrategyFactory.AnclaDesdeFechaInicio(abono.FechaInicio);

    /// <summary>
    /// Resuelve el período cobrable que contiene la fecha (1ª cuota irregular o ciclo natural).
    /// </summary>
    private static PeriodoCobro ResolvePeriodoParaFecha(Abono abono, DateOnly fechaRef)
    {
        var ancla = Ancla(abono);
        var strategy = PeriodicidadStrategyFactory.For(abono.PeriodicidadCobro);
        var primer = strategy.ResolverPrimerPeriodo(
            abono.FechaInicio, abono.PoliticaPrimerPeriodo, 0m, ancla);

        if (primer.Periodo.Contiene(fechaRef) || fechaRef == abono.FechaInicioCobro)
            return new PeriodoCobro(abono.FechaInicioCobro, primer.Periodo.Fin);

        return strategy.CicloQueContiene(fechaRef, ancla);
    }

    private static string FormatearPeriodo(DateOnly inicio, DateOnly fin, PeriodicidadCobro p) =>
        p switch
        {
            // Label por fin del slot (soporta 1ª cuota que arranca mid-quincena).
            PeriodicidadCobro.Quincenal => fin.Day <= 15
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
