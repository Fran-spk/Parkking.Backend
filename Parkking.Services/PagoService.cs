using System.Globalization;
using Parkking.DTOs.Pagos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;
using Parkking.Services.Movimientos;

namespace Parkking.Services;

public class PagoService
{
    private readonly PagoRepository _repository;
    private readonly CajaMensualService _cajaService;
    private readonly IEstacionamientoContext _estacionamiento;

    public PagoService(
        PagoRepository repository,
        CajaMensualService cajaService,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _cajaService = cajaService;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public void RegistrarPago(RegistrarPagoRequest request)
    {
        // El manejo de transacciones se mantiene acá para asegurar el rollback si falla la caja
        using var transaction = _repository.BeginTransaction();
        try
        {
            // 1. VALIDACIONES DE DOMINIO DEL ABONO
            var abono = _repository.GetAbonoActivo(request.AbonoCocheraId) // Si usas indices para optimizar
                ?? throw  new Exception("Abono no encontrado o inactivo.");

            if (!DateOnly.TryParseExact(request.Mes, "yyyy-MM-dd", out var mesNormalizado))
                throw new Exception("El formato de fecha debe ser yyyy-MM-dd.");

            var mesInicioCobro = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);
            if (mesNormalizado < mesInicioCobro)
                throw new Exception("No se puede registrar un pago anterior al inicio de los cobros del abono.");

            if (_repository.ExistePago(request.AbonoCocheraId, mesNormalizado.Year, mesNormalizado.Month))
                throw new Exception("Ya existe un pago registrado para ese mes.");

            // 2. CREACIÓN Y PERSISTENCIA DEL PAGO MENSUAL
            var pago = new PagoMensual
            {
                AbonoCocheraId = request.AbonoCocheraId,
                Mes = mesNormalizado,
                FechaHoraCarga = DateTime.UtcNow,
                Monto = request.Monto,
                Recargo = request.Recargo,
                Observacion = request.Observacion,
                MercadoPagoId = request.MercadoPagoId
            };

            _repository.AddPago(pago);
            _repository.SaveChanges(); 
            var estrategiaPago = new PagoMensualStrategy(pago, abono);
            _cajaService.RegistrarMovimiento(estrategiaPago );

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public PagoSugeridoDto GetSugerido(int abonoCocheraId, DateOnly? mes)
    {
        var abono = _repository.GetAbonoActivo(abonoCocheraId) ?? throw new Exception("Abono no encontrado.");
        var estacionamiento = _repository.GetEstacionamiento(TenantId);

        var tarifaBase = abono.PrecioAcordado ?? _repository.ObtenerTarifaVigente(
            abono.TipoVehiculoId, abono.Cochera.CategoriaCocheraId, TenantId, DateTime.UtcNow);

        var mesHoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var mesAPagar = mes.HasValue
            ? new DateOnly(mes.Value.Year, mes.Value.Month, 1)
            : new DateOnly(mesHoy.Year, mesHoy.Month, 1);

        var mesInicioCobro = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);
        var aplicaProporcional = false;
        var monto = tarifaBase;

        if (mesAPagar == mesInicioCobro &&
            abono.FechaInicio.Month == abono.FechaInicioCobro.Month &&
            abono.FechaInicio.Year == abono.FechaInicioCobro.Year &&
            estacionamiento?.DiasUmbralProporcional != null)
        {
            var dia = abono.FechaInicio.Day;
            if (dia > estacionamiento.DiasUmbralProporcional)
            {
                var diasMes = DateTime.DaysInMonth(abono.FechaInicio.Year, abono.FechaInicio.Month);
                var diasRestantes = diasMes - dia + 1;
                monto = Math.Round((tarifaBase / diasMes) * diasRestantes, 0);
                aplicaProporcional = true;
            }
        }

        decimal recargo = 0;
        if (estacionamiento?.AplicaRecargo == true)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            var vencimiento = new DateOnly(mesAPagar.Year, mesAPagar.Month, estacionamiento.DiaVencimientoAbono);
            if (hoy > vencimiento)
                recargo = Math.Round(monto * estacionamiento.PorcentajeRecargo / 100, 2);
        }

        return new PagoSugeridoDto
        {
            Monto = monto,
            Recargo = recargo,
            AplicaProporcional = aplicaProporcional,
            EsPrecioAcordado = abono.PrecioAcordado.HasValue,
            MesDate = mesAPagar
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
            var cursor = new DateOnly(abono.FechaInicioCobro.Year, abono.FechaInicioCobro.Month, 1);
            var limite = new DateOnly(hoy.Year, hoy.Month, 1);

            while (cursor <= limite)
            {
                var tienePago = abono.PagosMensuales.Any(p => p.Mes.Year == cursor.Year && p.Mes.Month == cursor.Month);
                if (!tienePago)
                {
                    var monto = abono.PrecioAcordado ?? _repository.ObtenerTarifaVigente(
                        abono.TipoVehiculoId, abono.Cochera.CategoriaCocheraId, TenantId, DateTime.UtcNow);

                    mesesImpagos.Add(new MesImpagoDto
                    {
                        AbonoCocheraId = abono.AbonoCocheraId,
                        NumeroCochera = abono.Cochera.Numero,
                        TipoVehiculo = abono.TipoVehiculo.Nombre,
                        Mes = cursor.ToString("MMMM yyyy", new CultureInfo("es-AR")),
                        MesDate = cursor,
                        Monto = monto,
                        Recargo = 0,
                        Total = monto
                    });
                }
                cursor = cursor.AddMonths(1);
            }
        }

        return new DeudaClienteDto
        {
            ClienteId = clienteId,
            NombreCliente = cliente.Nombre,
            MesesImpagos = mesesImpagos
        };
    }

    public List<PagoMensual> GetByAbono(int abonoId) => _repository.GetByAbono(abonoId);
    public List<PagoMensual> GetByCliente(int clienteId) => _repository.GetByCliente(clienteId);
    public List<PagoMensual> GetAll(DateOnly? desde, DateOnly? hasta) => _repository.GetAll(desde, hasta);
}