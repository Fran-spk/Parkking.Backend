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

    public Cochera Create(CocheraRequest request)
    {
        if (_repository.ExisteNumero(TenantId, request.Numero))
            throw new Exception($"Ya existe una cochera con el número {request.Numero}");

        if (request.EstadoCochera == EstadoCochera.Habilitada && !string.IsNullOrEmpty(request.Observacion))
            throw new Exception("No se puede cargar observación si la cochera está habilitada");

        var cochera = new Cochera
        {
            EstacionamientoId = TenantId,
            Numero = request.Numero,
            Observacion = request.Observacion,
            EstadoCochera = request.EstadoCochera,
            CategoriaCocheraId = request.CategoriaCocheraId,
            MultipleOcupacion = request.MultipleOcupacion,
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

        cochera.Numero = request.Numero;
        cochera.Observacion = request.Observacion;
        cochera.EstadoCochera = request.EstadoCochera;
        cochera.CategoriaCocheraId = request.CategoriaCocheraId;
        cochera.MultipleOcupacion = request.MultipleOcupacion;
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
        if (cochera.Abonos.Any(a => a.Activo))
            throw new Exception("No se puede desactivar una cochera con abonos activos");

        cochera.Activo = false;
        _repository.SaveChanges();
    }
}
