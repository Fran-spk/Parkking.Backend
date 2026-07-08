using Parkking.DTOs.Categorias;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class CategoriaService
{
    private readonly CategoriaRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public CategoriaService(CategoriaRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<CategoriaCochera> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(TenantId, includeInactivos);

    public CategoriaCochera? GetById(int id) => _repository.GetById(id, TenantId);

    public CategoriaCochera Create(CategoriaCocheraRequest request)
    {
        if (_repository.ExisteNombre(request.Nombre, TenantId))
            throw new Exception("Ya existe una categoría con ese nombre");

        var categoria = new CategoriaCochera
        {
            EstacionamientoId = TenantId,
            Nombre = request.Nombre,
            Activo = true
        };
        _repository.Add(categoria);
        _repository.SaveChanges();
        return categoria;
    }

    public CategoriaCochera Update(int id, CategoriaCocheraRequest request)
    {
        var categoria = _repository.GetById(id, TenantId) ?? throw new Exception("Categoría no encontrada");
        if (!categoria.Activo) throw new Exception("La categoría está dada de baja");
        if (_repository.ExisteNombre(request.Nombre, TenantId, id))
            throw new Exception("Ya existe una categoría con ese nombre");

        categoria.Nombre = request.Nombre;
        _repository.SaveChanges();
        return categoria;
    }

    public void Deactivate(int id)
    {
        var categoria = _repository.GetById(id, TenantId) ?? throw new Exception("Categoría no encontrada");
        if (!categoria.Activo) throw new Exception("La categoría ya está dada de baja");
        if (_repository.TieneCocherasActivas(id))
            throw new Exception("No se puede dar de baja, hay cocheras activas con esta categoría");

        categoria.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var categoria = _repository.GetById(id, TenantId) ?? throw new Exception("Categoría no encontrada");
        categoria.Activo = true;
        _repository.SaveChanges();
    }
}
