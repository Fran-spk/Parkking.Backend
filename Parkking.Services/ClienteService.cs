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

    public List<VehiculoDto> GetVehiculos(int clienteId, bool soloDisponibles = false)
    {
        var cliente = _repository.GetById(clienteId) ?? throw new Exception("Cliente no encontrado");
        return _repository.GetVehiculosByCliente(cliente.ClienteId)
            .Select(v =>
            {
                var asignado = v.AbonoVehiculo != null && v.AbonoVehiculo.Abono != null && v.AbonoVehiculo.Abono.Activo;
                return new VehiculoDto
                {
                    VehiculoId = v.VehiculoId,
                    ClienteId = v.ClienteId,
                    Patente = v.Patente,
                    ModeloVehiculo = v.ModeloVehiculo,
                    TipoVehiculoId = v.TipoVehiculoId,
                    TipoVehiculoNombre = v.TipoVehiculo?.Nombre,
                    Activo = v.Activo,
                    AsignadoAAbonoActivo = asignado,
                };
            })
            .Where(v => !soloDisponibles || !v.AsignadoAAbonoActivo)
            .ToList();
    }

    public Cliente Create(ClienteRequest request)
    {
        var cliente = new Cliente
        {
            EstacionamientoId = _estacionamiento.EstacionamientoId,
            Nombre = request.Nombre,
            Documento = string.IsNullOrWhiteSpace(request.Documento) ? null : request.Documento.Trim(),
            Domicilio = string.IsNullOrWhiteSpace(request.Domicilio) ? null : request.Domicilio.Trim(),
            Telefono = request.Telefono,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
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
        cliente.Documento = string.IsNullOrWhiteSpace(request.Documento) ? null : request.Documento.Trim();
        cliente.Domicilio = string.IsNullOrWhiteSpace(request.Domicilio) ? null : request.Domicilio.Trim();
        cliente.Telefono = request.Telefono;
        cliente.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
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
