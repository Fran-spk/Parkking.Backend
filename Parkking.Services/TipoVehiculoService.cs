using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class TipoVehiculoService
{
    private readonly TipoVehiculoRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public TipoVehiculoService(TipoVehiculoRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<TipoVehiculo> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(TenantId, includeInactivos);

    public TipoVehiculo? GetById(int id) => _repository.GetById(id, TenantId);

    public TipoVehiculo Create(string nombre)
    {
        if (_repository.ExisteNombre(nombre, TenantId))
            throw new Exception("Ya existe un tipo de vehículo con ese nombre");

        var tipo = new TipoVehiculo
        {
            EstacionamientoId = TenantId,
            Nombre = nombre,
            Activo = true
        };
        _repository.Add(tipo);
        _repository.SaveChanges();
        return tipo;
    }

    public TipoVehiculo Update(int id, string nuevoNombre)
    {
        var tipo = _repository.GetById(id, TenantId) ?? throw new Exception("Tipo de vehículo no encontrado");
        if (!tipo.Activo) throw new Exception("El tipo de vehículo está dado de baja");
        if (_repository.ExisteNombre(nuevoNombre, TenantId, id))
            throw new Exception("Ya existe un tipo de vehículo con ese nombre");

        tipo.Nombre = nuevoNombre;
        _repository.SaveChanges();
        return tipo;
    }

    public void Deactivate(int id)
    {
        var tipo = _repository.GetById(id, TenantId) ?? throw new Exception("Tipo de vehículo no encontrado");
        if (!tipo.Activo) throw new Exception("El tipo de vehículo ya está dado de baja");
        if (_repository.TieneAbonosActivos(id))
            throw new Exception("No se puede dar de baja, existen abonos activos con este tipo de vehículo");

        tipo.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var tipo = _repository.GetById(id, TenantId) ?? throw new Exception("Tipo de vehículo no encontrado");
        tipo.Activo = true;
        _repository.SaveChanges();
    }
}
