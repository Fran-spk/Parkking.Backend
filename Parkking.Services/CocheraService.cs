using Parkking.DTOs.Cocheras;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class CocheraService
{
    private readonly CocheraRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public CocheraService(CocheraRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<Cochera> GetAll() => _repository.GetAllActivas(TenantId);

    public Cochera? GetById(int id) => _repository.GetById(id, TenantId);

    public List<Cochera> GetLibresByTipoVehiculo(int tipoVehiculoId) =>
        _repository.GetActivas(TenantId)
            .Where(c => c.EstadoCochera == EstadoCochera.Habilitada
                     && c.VehiculosPermitidos.Any(v => v.TipoVehiculoId == tipoVehiculoId)
                     && c.EstaDisponible())
            .OrderBy(c => c.Numero).ToList();

    /// <summary>
    /// Cochera activa del tenant con cupo libre para un abono más.
    /// </summary>
    public Cochera ValidarDisponible(int cocheraId)
    {
        var cochera = _repository.GetById(cocheraId, TenantId)
            ?? throw new Exception($"Cochera {cocheraId} no encontrada o inactiva");

        var activos = cochera.ContarAbonosActivos();
        var cupo = cochera.CapacidadMaxima();
        if (activos >= cupo)
        {
            if (!cochera.MultipleOcupacion)
                throw new Exception($"La cochera {cochera.Numero} no admite múltiples ocupaciones");

            throw new Exception(
                $"La cochera {cochera.Numero} alcanzó el máximo de ocupación ({activos}/{cupo})");
        }

        return cochera;
    }

    public Cochera Create(CocheraRequest request)
    {
        if (_repository.ExisteNumero(TenantId, request.Numero))
            throw new Exception($"Ya existe una cochera con el número {request.Numero}");

        if (request.EstadoCochera == EstadoCochera.Habilitada && !string.IsNullOrEmpty(request.Observacion))
            throw new Exception("No se puede cargar observación si la cochera está habilitada");

        var maxOcupacion = ResolverMaxOcupacion(request);

        var cochera = new Cochera
        {
            EstacionamientoId = TenantId,
            Numero = request.Numero,
            Observacion = request.Observacion,
            EstadoCochera = request.EstadoCochera,
            CategoriaCocheraId = request.CategoriaCocheraId,
            MultipleOcupacion = request.MultipleOcupacion,
            MaxOcupacion = maxOcupacion,
            Activo = true
        };

        if (request.VehiculosPermitidosIds?.Any() == true)
            cochera.VehiculosPermitidos = _repository.GetTiposVehiculo(request.VehiculosPermitidosIds);

        _repository.Add(cochera);
        _repository.SaveChanges();
        return cochera;
    }

    public Cochera Update(int id, CocheraRequest request)
    {
        var cochera = _repository.GetForUpdate(id, TenantId) ?? throw new Exception("Cochera no encontrada");

        if (request.EstadoCochera == EstadoCochera.Habilitada && !string.IsNullOrEmpty(request.Observacion))
            throw new Exception("No se puede cargar observación si la cochera está habilitada");

        var maxOcupacion = ResolverMaxOcupacion(request);
        var activos = cochera.ContarAbonosActivos();
        if (request.MultipleOcupacion)
        {
            var cupo = maxOcupacion ?? 2;
            if (activos > cupo)
                throw new Exception(
                    $"La cochera ya tiene {activos} abonos activos; el máximo no puede ser menor ({cupo}).");
        }
        else if (activos > 1)
        {
            throw new Exception(
                $"La cochera tiene {activos} abonos activos; no se puede desactivar la múltiple ocupación.");
        }

        cochera.Numero = request.Numero;
        cochera.Observacion = request.Observacion;
        cochera.EstadoCochera = request.EstadoCochera;
        cochera.CategoriaCocheraId = request.CategoriaCocheraId;
        cochera.MultipleOcupacion = request.MultipleOcupacion;
        cochera.MaxOcupacion = maxOcupacion;
        cochera.VehiculosPermitidos.Clear();

        if (request.VehiculosPermitidosIds != null)
            foreach (var v in _repository.GetTiposVehiculo(request.VehiculosPermitidosIds))
                cochera.VehiculosPermitidos.Add(v);

        _repository.SaveChanges();
        return cochera;
    }

    public void Deactivate(int id)
    {
        var cochera = _repository.GetForDeactivate(id, TenantId) ?? throw new Exception("Cochera no encontrada");
        if (cochera.Plazas.Any(p => p.Activo && p.Abono.Activo))
            throw new Exception("No se puede desactivar una cochera con abonos activos");

        cochera.Activo = false;
        _repository.SaveChanges();
    }

    private static int? ResolverMaxOcupacion(CocheraRequest request)
    {
        if (!request.MultipleOcupacion)
            return null;

        var max = request.MaxOcupacion ?? 2;
        if (max < 2)
            throw new Exception("Con múltiple ocupación el máximo debe ser al menos 2.");
        if (max > 50)
            throw new Exception("El máximo de ocupación no puede superar 50.");

        return max;
    }
}
