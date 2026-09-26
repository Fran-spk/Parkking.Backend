using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

public class TipoGastoService
{
    private readonly TipoGastoRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public TipoGastoService(TipoGastoRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<TipoGastoDto> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(TenantId, includeInactivos).Select(Map).ToList();

    public TipoGastoDto? GetById(int id)
    {
        var tipo = _repository.GetById(id, TenantId);
        return tipo == null ? null : Map(tipo);
    }

    public TipoGastoDto Create(CrearTipoGastoRequest request)
    {
        var nombre = ValidarNombre(request.Nombre);
        if (_repository.ExisteNombreActivo(nombre, TenantId))
            throw new Exception("Ya existe un tipo de gasto activo con ese nombre.");

        var tipo = new TipoGasto
        {
            EstacionamientoId = TenantId,
            Nombre = nombre,
            Activo = true
        };
        _repository.Add(tipo);
        _repository.SaveChanges();
        return Map(tipo);
    }

    public TipoGastoDto Update(int id, EditarTipoGastoRequest request)
    {
        var tipo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Tipo de gasto no encontrado.");

        var nombre = ValidarNombre(request.Nombre);
        if (_repository.ExisteNombreActivo(nombre, TenantId, id))
            throw new Exception("Ya existe un tipo de gasto activo con ese nombre.");

        tipo.Nombre = nombre;
        _repository.SaveChanges();
        return Map(tipo);
    }

    public void Deactivate(int id)
    {
        var tipo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Tipo de gasto no encontrado.");
        if (!tipo.Activo)
            throw new Exception("El tipo de gasto ya está dado de baja.");

        tipo.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var tipo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Tipo de gasto no encontrado.");
        if (tipo.Activo)
            throw new Exception("El tipo de gasto ya está activo.");
        if (_repository.ExisteNombreActivo(tipo.Nombre, TenantId, id))
            throw new Exception("Ya existe un tipo de gasto activo con ese nombre.");

        tipo.Activo = true;
        _repository.SaveChanges();
    }

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("Debe ingresar un nombre.");
        var n = nombre.Trim();
        if (n.Length > 80)
            throw new Exception("El nombre no puede superar 80 caracteres.");
        return n;
    }

    private static TipoGastoDto Map(TipoGasto t) => new()
    {
        TipoGastoId = t.TipoGastoId,
        Nombre = t.Nombre,
        Activo = t.Activo
    };
}
