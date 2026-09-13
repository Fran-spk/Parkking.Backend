using Parkking.DTOs.Abonos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;

namespace Parkking.Services;

public class AbonoService
{
    private readonly AbonoRepository _repository;
    private readonly AbonoPrecioService _precioService;
    private readonly IEstacionamientoContext _estacionamiento;

    public AbonoService(
        AbonoRepository repository,
        AbonoPrecioService precioService,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _precioService = precioService;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<Abono> GetAll() => _repository.GetAll(TenantId);
    public List<Abono> GetActivos() => _repository.GetActivos(TenantId);
    public List<Abono> GetByCliente(int clienteId) => _repository.GetByCliente(clienteId, TenantId);
    public List<Abono> GetByCochera(int cocheraId) => _repository.GetByCochera(cocheraId);

    public Abono GetById(int id) =>
        _repository.LoadWithIncludes(id) ?? throw new Exception("Abono no encontrado");

    public Abono GetByPatente(string patente)
    {
        var abono = _repository.GetByPatente(NormalizarPatente(patente), TenantId);
        if (abono == null) throw new Exception("No se encontró ningún abono con esa patente");
        return abono;
    }

    /// <summary>Búsqueda por patente (parcial). Devuelve abonos del tenant.</summary>
    public List<Abono> BuscarPorPatente(string termino)
    {
        var q = NormalizarPatente(termino);
        if (q.Length < 2)
            throw new Exception("Ingresá al menos 2 caracteres de la patente.");

        return _repository.BuscarPorPatente(q, TenantId);
    }

    private static string NormalizarPatente(string? patente) =>
        (patente ?? string.Empty).Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

    public Abono Create(CrearAbonoRequest request)
    {
        if (_repository.GetCliente(request.ClienteId) == null)
            throw new Exception("Cliente no encontrado");

        var cocheraIds = ResolveCocheraIds(request);
        if (cocheraIds.Count == 0)
            throw new Exception("El abono debe incluir al menos una cochera");

        var vehiculosReq = ResolveVehiculos(request);
        ValidarPatentesUnicasEnRequest(vehiculosReq);

        if (vehiculosReq.Any(v => v.Modalidad == ModalidadVehiculoAbono.Flexible)
            && (!request.PrecioAcordado.HasValue || request.PrecioAcordado.Value <= 0))
        {
            throw new Exception(
                "Los abonos con vehículos flexibles requieren precio acordado.");
        }

        var (fechaInicio, fechaInicioCobro) = ParseFechas(request.FechaInicio, request.FechaInicioCobro);

        // Normalizar inicio de cobro al inicio del período según periodicidad
        var (periodoInicioCobro, periodoFinCobro) =
            PeriodicidadHelper.PeriodoQueContiene(fechaInicioCobro, request.PeriodicidadCobro);
        fechaInicioCobro = periodoInicioCobro;

        foreach (var cocheraId in cocheraIds)
            ValidarCocheraDisponible(cocheraId);

        var abono = new Abono
        {
            ClienteId = request.ClienteId,
            EstacionamientoId = TenantId,
            Cobrador = request.Cobrador,
            PrecioAcordado = request.PrecioAcordado,
            PeriodicidadCobro = request.PeriodicidadCobro,
            Activo = true,
            FechaInicio = fechaInicio,
            FechaInicioCobro = fechaInicioCobro
        };

        foreach (var cocheraId in cocheraIds)
        {
            abono.Plazas.Add(new AbonoPlaza
            {
                EstacionamientoId = TenantId,
                CocheraId = cocheraId,
                Activo = true
            });
        }

        _repository.Add(abono);
        _repository.SaveChanges();

        foreach (var vehiculoReq in vehiculosReq)
            AsignarVehiculoInterno(abono.AbonoId, request.ClienteId, vehiculoReq);

        // Recargar con plazas/vehículos para calcular monto y generar 1ª cuota
        abono = _repository.LoadWithIncludes(abono.AbonoId)!;
        GenerarPrimeraCuota(abono, periodoInicioCobro, periodoFinCobro);

        return _repository.LoadWithIncludes(abono.AbonoId)!;
    }

    public Abono Update(int id, ModificarAbonoRequest request)
    {
        var abono = _repository.LoadWithIncludes(id) ?? throw new Exception("Abono no encontrado");
        if (!abono.Activo) throw new Exception("Abono no encontrado");

        var nuevoPrecio = request.PrecioAcordado;
        if (AbonoPrecioService.TieneVehiculoFlexible(abono)
            && (!nuevoPrecio.HasValue || nuevoPrecio.Value <= 0))
        {
            throw new Exception(
                "Los abonos con vehículos flexibles requieren precio acordado.");
        }

        abono.Cobrador = request.Cobrador;
        abono.PrecioAcordado = request.PrecioAcordado;
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(id)!;
    }

    public Abono AgregarPlaza(int abonoId, AgregarPlazaRequest request)
    {
        var abono = _repository.GetActivoWithPlazas(abonoId) ?? throw new Exception("Abono no encontrado");
        if (_repository.ExistePlazaActiva(abonoId, request.CocheraId))
            throw new Exception("La cochera ya está asignada a este abono");

        ValidarCocheraDisponible(request.CocheraId);

        _repository.AddPlaza(new AbonoPlaza
        {
            EstacionamientoId = TenantId,
            AbonoId = abonoId,
            CocheraId = request.CocheraId,
            Activo = true
        });
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(abonoId)!;
    }

    public Abono RemoverPlaza(int abonoId, int abonoPlazaId)
    {
        var plaza = _repository.GetPlaza(abonoPlazaId)
            ?? throw new Exception("Plaza no encontrada");
        if (plaza.AbonoId != abonoId || !plaza.Activo)
            throw new Exception("La plaza no pertenece a este abono");

        if (plaza.VehiculosFijos.Any())
            throw new Exception("No se puede quitar la plaza: tiene vehículos fijos asignados. Reasignalos o dalos de baja antes.");

        var abono = _repository.GetActivoWithPlazas(abonoId)
            ?? throw new Exception("Abono no encontrado");
        if (abono.Plazas.Count(p => p.Activo) <= 1)
            throw new Exception("El abono debe conservar al menos una plaza activa");

        plaza.Activo = false;
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(abonoId)!;
    }

    public Abono MoverPlaza(int abonoId, int abonoPlazaId, int nuevaCocheraId)
    {
        var plaza = _repository.GetPlaza(abonoPlazaId)
            ?? throw new Exception("Plaza no encontrada");
        if (plaza.AbonoId != abonoId || !plaza.Activo)
            throw new Exception("La plaza no pertenece a este abono");

        if (plaza.CocheraId == nuevaCocheraId)
            return _repository.LoadWithIncludes(abonoId)!;

        if (_repository.ExistePlazaActiva(abonoId, nuevaCocheraId))
            throw new Exception("La cochera destino ya está en este abono");

        var nuevaCochera = ValidarCocheraDisponible(nuevaCocheraId);

        foreach (var fijo in plaza.VehiculosFijos)
            ValidarTipoPermitido(nuevaCochera, fijo.Vehiculo.TipoVehiculoId);

        plaza.CocheraId = nuevaCocheraId;
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(abonoId)!;
    }

    /// <summary>Compat: mueve la primera plaza activa.</summary>
    public void MoveCochera(int id, int nuevaCocheraId)
    {
        var abono = _repository.GetActivoWithPlazas(id) ?? throw new Exception("Abono no encontrado");
        var plaza = abono.Plazas.FirstOrDefault(p => p.Activo)
            ?? throw new Exception("El abono no tiene plazas activas");
        MoverPlaza(id, plaza.AbonoPlazaId, nuevaCocheraId);
    }

    public Abono AgregarVehiculo(int abonoId, AsignarVehiculoAbonoRequest request)
    {
        var abono = _repository.LoadWithIncludes(abonoId) ?? throw new Exception("Abono no encontrado");
        if (!abono.Activo) throw new Exception("Abono no encontrado");

        if (request.Modalidad == ModalidadVehiculoAbono.Flexible
            && (!abono.PrecioAcordado.HasValue || abono.PrecioAcordado.Value <= 0))
        {
            throw new Exception(
                "Para asignar un vehículo flexible el abono debe tener precio acordado.");
        }

        AsignarVehiculoInterno(abonoId, abono.ClienteId, request);
        return _repository.LoadWithIncludes(abonoId)!;
    }

    public Abono ModificarVehiculo(int abonoId, int abonoVehiculoId, ModificarVehiculoAbonoRequest request)
    {
        var vinculo = _repository.GetAbonoVehiculo(abonoVehiculoId)
            ?? throw new Exception("Vínculo de vehículo no encontrado");
        if (vinculo.AbonoId != abonoId)
            throw new Exception("El vehículo no pertenece a este abono");

        if (!string.IsNullOrWhiteSpace(request.Patente))
        {
            var patente = NormalizarPatente(request.Patente);
            if (_repository.PatenteEnAbono(abonoId, patente, abonoVehiculoId))
                throw new Exception("Ya existe un vehículo con esa patente en el abono");

            if (_repository.PatenteEnAbonoActivo(TenantId, patente, abonoId, vinculo.VehiculoId))
                throw new Exception("Esa patente ya está en otro abono activo");

            var otro = _repository.GetVehiculoByPatenteEnEstacionamiento(patente, TenantId);
            if (otro != null && otro.VehiculoId != vinculo.VehiculoId)
                throw new Exception("Ya existe un vehículo con esa patente en el estacionamiento");

            vinculo.Vehiculo.Patente = patente;
        }

        if (request.ModeloVehiculo != null)
            vinculo.Vehiculo.ModeloVehiculo = request.ModeloVehiculo;

        if (request.TipoVehiculoId.HasValue)
            vinculo.Vehiculo.TipoVehiculoId = request.TipoVehiculoId.Value;

        var modalidad = request.Modalidad ?? vinculo.Modalidad;
        var plaza = ResolverPlaza(abonoId, request.AbonoPlazaId, request.CocheraId, modalidad, optional: true);

        if (request.Modalidad.HasValue || request.AbonoPlazaId.HasValue || request.CocheraId.HasValue)
        {
            if (modalidad == ModalidadVehiculoAbono.Fijo)
            {
                plaza ??= vinculo.AbonoPlaza
                    ?? throw new Exception("Un vehículo fijo requiere plaza (AbonoPlazaId o CocheraId)");
                ValidarTipoPermitido(plaza.Cochera, vinculo.Vehiculo.TipoVehiculoId);
                vinculo.Modalidad = ModalidadVehiculoAbono.Fijo;
                vinculo.AbonoPlazaId = plaza.AbonoPlazaId;
            }
            else
            {
                vinculo.Modalidad = ModalidadVehiculoAbono.Flexible;
                vinculo.AbonoPlazaId = null;
            }
        }

        _repository.SaveChanges();
        return _repository.LoadWithIncludes(abonoId)!;
    }

    public Abono RemoverVehiculo(int abonoId, int abonoVehiculoId)
    {
        var vinculo = _repository.GetAbonoVehiculo(abonoVehiculoId)
            ?? throw new Exception("Vínculo de vehículo no encontrado");
        if (vinculo.AbonoId != abonoId)
            throw new Exception("El vehículo no pertenece a este abono");

        _repository.RemoveAbonoVehiculo(vinculo);
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(abonoId)!;
    }

    public void Deactivate(int id)
    {
        var abono = _repository.GetActivoWithPlazas(id) ?? throw new Exception("Abono no encontrado");
        abono.Activo = false;
        foreach (var plaza in abono.Plazas.Where(p => p.Activo))
            plaza.Activo = false;
        // Libera vehículos (único por VehiculoId) para poder usarlos en otro abono.
        foreach (var av in abono.AbonoVehiculos.ToList())
            _repository.RemoveAbonoVehiculo(av);
        _repository.SaveChanges();
    }

    private void AsignarVehiculoInterno(int abonoId, int clienteId, AsignarVehiculoAbonoRequest request)
    {
        var abono = _repository.GetActivoWithPlazas(abonoId)
            ?? throw new Exception("Abono no encontrado");

        Vehiculo vehiculo;
        if (request.VehiculoId.HasValue)
        {
            vehiculo = _repository.GetVehiculo(request.VehiculoId.Value)
                ?? throw new Exception("Vehículo no encontrado");
            if (vehiculo.ClienteId != clienteId)
                throw new Exception("El vehículo no pertenece al cliente del abono");
            if (vehiculo.EstacionamientoId != TenantId)
                throw new Exception("El vehículo no pertenece a este estacionamiento");
            if (_repository.VehiculoAsignadoAAbonoActivo(vehiculo.VehiculoId, abonoId))
                throw new Exception("Ese vehículo ya está asignado a otro abono activo");

            var patenteNorm = NormalizarPatente(vehiculo.Patente);
            if (_repository.PatenteEnAbonoActivo(TenantId, patenteNorm, abonoId, vehiculo.VehiculoId))
                throw new Exception("Esa patente ya está en otro abono activo");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.Patente))
                throw new Exception("La patente es obligatoria para crear un vehículo");
            if (!request.TipoVehiculoId.HasValue)
                throw new Exception("TipoVehiculoId es obligatorio para crear un vehículo");

            var patente = NormalizarPatente(request.Patente);
            if (patente.Length < 2)
                throw new Exception("La patente no es válida");

            if (_repository.PatenteEnAbono(abonoId, patente))
                throw new Exception("Ya existe un vehículo con esa patente en el abono");

            if (_repository.PatenteEnAbonoActivo(TenantId, patente, abonoId))
                throw new Exception("Esa patente ya está en otro abono activo");

            var existente = _repository.GetVehiculoByPatenteEnEstacionamiento(patente, TenantId);
            if (existente != null)
            {
                if (existente.ClienteId != clienteId)
                    throw new Exception("Ya existe un vehículo con esa patente en el estacionamiento");

                if (_repository.VehiculoAsignadoAAbonoActivo(existente.VehiculoId, abonoId))
                    throw new Exception("Esa patente ya está asignada a otro abono activo");

                vehiculo = existente;
                if (request.ModeloVehiculo != null)
                    vehiculo.ModeloVehiculo = request.ModeloVehiculo;
                if (request.TipoVehiculoId.HasValue)
                    vehiculo.TipoVehiculoId = request.TipoVehiculoId.Value;
                vehiculo.Patente = patente;
                vehiculo.Activo = true;
            }
            else
            {
                vehiculo = new Vehiculo
                {
                    EstacionamientoId = TenantId,
                    ClienteId = clienteId,
                    Patente = patente,
                    ModeloVehiculo = request.ModeloVehiculo,
                    TipoVehiculoId = request.TipoVehiculoId.Value,
                    Activo = true
                };
                _repository.AddVehiculo(vehiculo);
                _repository.SaveChanges();
            }
        }

        AbonoPlaza? plaza = null;
        if (request.Modalidad == ModalidadVehiculoAbono.Fijo)
        {
            plaza = ResolverPlaza(abonoId, request.AbonoPlazaId, request.CocheraId, ModalidadVehiculoAbono.Fijo)
                ?? throw new Exception("Un vehículo fijo requiere plaza (AbonoPlazaId o CocheraId)");
            ValidarTipoPermitido(plaza.Cochera, vehiculo.TipoVehiculoId);
        }
        else if (abono.Plazas.Any(p => p.Activo))
        {
            // Flexible: el tipo debe ser admitido en al menos una plaza del abono
            var permitido = abono.Plazas
                .Where(p => p.Activo)
                .Any(p => p.Cochera.VehiculosPermitidos.Any(t => t.TipoVehiculoId == vehiculo.TipoVehiculoId)
                       || !p.Cochera.VehiculosPermitidos.Any());
            if (!permitido)
                throw new Exception("El tipo de vehículo no está permitido en ninguna plaza del abono");
        }

        // Vínculo residual de abono inactivo bloquearía el índice único VehiculoId.
        if (vehiculo.AbonoVehiculo != null)
            _repository.RemoveAbonoVehiculo(vehiculo.AbonoVehiculo);

        _repository.AddAbonoVehiculo(new AbonoVehiculo
        {
            EstacionamientoId = TenantId,
            AbonoId = abonoId,
            VehiculoId = vehiculo.VehiculoId,
            Modalidad = request.Modalidad,
            AbonoPlazaId = plaza?.AbonoPlazaId
        });
        _repository.SaveChanges();
    }

    private AbonoPlaza? ResolverPlaza(
        int abonoId,
        int? abonoPlazaId,
        int? cocheraId,
        ModalidadVehiculoAbono modalidad,
        bool optional = false)
    {
        if (modalidad == ModalidadVehiculoAbono.Flexible)
            return null;

        if (abonoPlazaId.HasValue)
        {
            var plaza = _repository.GetPlaza(abonoPlazaId.Value)
                ?? throw new Exception("Plaza no encontrada");
            if (plaza.AbonoId != abonoId || !plaza.Activo)
                throw new Exception("La plaza no pertenece a este abono");
            return plaza;
        }

        if (cocheraId.HasValue)
        {
            var abono = _repository.GetActivoWithPlazas(abonoId)
                ?? throw new Exception("Abono no encontrado");
            return abono.Plazas.FirstOrDefault(p => p.Activo && p.CocheraId == cocheraId.Value)
                ?? throw new Exception("No hay una plaza activa con esa cochera en el abono");
        }

        if (optional)
            return null;

        throw new Exception("Se requiere AbonoPlazaId o CocheraId para modalidad Fijo");
    }

    private Cochera ValidarCocheraDisponible(int cocheraId)
    {
        var cochera = _repository.GetCocheraDestino(cocheraId, TenantId)
            ?? throw new Exception($"Cochera {cocheraId} no encontrada o inactiva");

        var activos = cochera.Plazas.Count(p => p.Activo && p.Abono.Activo);
        if (activos > 0 && !cochera.MultipleOcupacion)
            throw new Exception($"La cochera {cochera.Numero} no admite múltiples ocupaciones");

        return cochera;
    }

    private static void ValidarTipoPermitido(Cochera cochera, int tipoVehiculoId)
    {
        if (!cochera.VehiculosPermitidos.Any())
            return;
        if (!cochera.VehiculosPermitidos.Any(v => v.TipoVehiculoId == tipoVehiculoId))
            throw new Exception($"El tipo de vehículo no está permitido en la cochera {cochera.Numero}");
    }

    private void GenerarPrimeraCuota(Abono abono, DateOnly periodoInicio, DateOnly periodoFin)
    {
        if (abono.Cuotas.Any(c => c.Estado != EstadoCuota.Anulada && c.PeriodoInicio == periodoInicio))
            return;

        var precio = _precioService.ResolverParaPeriodo(abono, periodoInicio, periodoFin);
        var monto = CalcularMontoPrimeraCuota(abono, periodoInicio, precio.Monto);
        var detalles = _precioService.CrearDetallesLiquidacion(abono, precio, monto);

        _repository.AddCuota(new Cuota
        {
            AbonoId = abono.AbonoId,
            EstacionamientoId = abono.EstacionamientoId,
            PeriodoInicio = periodoInicio,
            PeriodoFin = periodoFin,
            Monto = monto,
            Estado = EstadoCuota.Pendiente,
            Detalles = detalles,
        });
        _repository.SaveChanges();
    }

    /// <summary>
    /// Monto de la 1ª cuota según DatosEstacionamiento (prorrateo por umbral).
    /// Sin recargo: la mora se calcula al cobrar.
    /// </summary>
    private decimal CalcularMontoPrimeraCuota(Abono abono, DateOnly periodoInicio, decimal montoBase)
    {
        var datos = _repository.GetEstacionamiento(TenantId)
            ?? throw new Exception("No se encontraron los datos del estacionamiento.");

        return datos.CalcularMontoPrimeraCuota(
            montoBase,
            abono.FechaInicio,
            periodoInicio,
            abono.PeriodicidadCobro);
    }

    private static List<int> ResolveCocheraIds(CrearAbonoRequest request)
    {
        var ids = request.CocheraIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
        if (ids.Count == 0 && request.CocheraId.HasValue && request.CocheraId.Value > 0)
            ids.Add(request.CocheraId.Value);
        return ids;
    }

    private static List<AsignarVehiculoAbonoRequest> ResolveVehiculos(CrearAbonoRequest request)
    {
        if (request.Vehiculos is { Count: > 0 })
            return request.Vehiculos;

        if (string.IsNullOrWhiteSpace(request.Patente))
            return new List<AsignarVehiculoAbonoRequest>();

        var cocheraId = ResolveCocheraIds(request).FirstOrDefault();
        return new List<AsignarVehiculoAbonoRequest>
        {
            new()
            {
                Patente = request.Patente,
                ModeloVehiculo = request.ModeloVehiculo,
                TipoVehiculoId = request.TipoVehiculoId,
                Modalidad = ModalidadVehiculoAbono.Fijo,
                CocheraId = cocheraId > 0 ? cocheraId : null
            }
        };
    }

    private void ValidarPatentesUnicasEnRequest(List<AsignarVehiculoAbonoRequest> vehiculos)
    {
        var vistas = new HashSet<string>(StringComparer.Ordinal);
        foreach (var v in vehiculos)
        {
            string clave;
            if (v.VehiculoId.HasValue)
                clave = $"id:{v.VehiculoId.Value}";
            else
            {
                var p = NormalizarPatente(v.Patente);
                if (string.IsNullOrEmpty(p))
                    continue;
                clave = $"p:{p}";
            }

            if (!vistas.Add(clave))
                throw new Exception("No se pueden repetir patentes en el mismo abono");
        }
    }

    private static (DateOnly FechaInicio, DateOnly FechaInicioCobro) ParseFechas(string fechaInicio, string? fechaInicioCobro)
    {
        if (!DateOnly.TryParseExact(fechaInicio, "yyyy-MM-dd", out var fechaInicioParsed))
            throw new Exception("El formato de FechaInicio debe ser yyyy-MM-dd");

        var fechaInicioCobroParsed = fechaInicioParsed;
        if (!string.IsNullOrEmpty(fechaInicioCobro))
        {
            if (!DateOnly.TryParseExact(fechaInicioCobro, "yyyy-MM-dd", out var parsedCobro))
                throw new Exception("El formato de FechaInicioCobro debe ser yyyy-MM-dd");
            fechaInicioCobroParsed = parsedCobro;
        }

        return (fechaInicioParsed, fechaInicioCobroParsed);
    }
}
