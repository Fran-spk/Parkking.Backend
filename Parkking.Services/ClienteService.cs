using Parkking.DTOs.Clientes;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class ClienteService
{
    private readonly ClienteRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public ClienteService(ClienteRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    public List<Cliente> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(_estacionamiento.EstacionamientoId, includeInactivos);

    public Cliente GetById(int id)
    {
        var cliente = _repository.GetByIdWithAbonos(id);
        if (cliente == null) throw new Exception("Cliente no encontrado");
        return cliente;
    }

    public Cliente Create(ClienteRequest request)
    {
        var cliente = new Cliente
        {
            EstacionamientoId = _estacionamiento.EstacionamientoId,
            Nombre = request.Nombre,
            Telefono = request.Telefono,
            Email = request.Email,
            Observacion = request.Observacion,
            Activo = true
        };
        _repository.Add(cliente);
        _repository.SaveChanges();
        return cliente;
    }

    public Cliente Update(int id, ClienteRequest request)
    {
        var cliente = _repository.GetById(id) ?? throw new Exception("Cliente no encontrado");
        if (!cliente.Activo) throw new Exception("El cliente está dado de baja");

        cliente.Nombre = request.Nombre;
        cliente.Telefono = request.Telefono;
        cliente.Email = request.Email;
        cliente.Observacion = request.Observacion;
        _repository.SaveChanges();
        return cliente;
    }

    public void Deactivate(int id)
    {
        var cliente = _repository.GetById(id) ?? throw new Exception("Cliente no encontrado");
        if (!cliente.Activo) throw new Exception("El cliente ya está dado de baja");
        if (_repository.TieneAbonosActivos(id))
            throw new Exception("No se puede dar de baja, el cliente tiene abonos activos");

        cliente.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var cliente = _repository.GetById(id) ?? throw new Exception("Cliente no encontrado");
        cliente.Activo = true;
        _repository.SaveChanges();
    }
}
