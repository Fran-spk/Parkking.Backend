using Parkking.DTOs.MetodosDePago;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class MetodoDePagoService
{
    public static readonly string[] NombresPorDefecto = ["Efectivo", "Transferencia", "Débito"];

    private readonly MetodoDePagoRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public MetodoDePagoService(MetodoDePagoRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<MetodoDePagoDto> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(TenantId, includeInactivos).Select(Map).ToList();

    public MetodoDePagoDto? GetById(int id)
    {
        var m = _repository.GetById(id, TenantId);
        return m == null ? null : Map(m);
    }

    public MetodoDePago RequireActivo(int metodoDePagoId)
    {
        var m = _repository.GetById(metodoDePagoId, TenantId)
            ?? throw new Exception("Método de pago no encontrado.");
        if (!m.Activo)
            throw new Exception("El método de pago está dado de baja.");
        return m;
    }

    public MetodoDePagoDto Create(string nombre)
    {
        var n = ValidarNombre(nombre);
        if (_repository.ExisteNombre(n, TenantId))
            throw new Exception("Ya existe un método de pago con ese nombre.");

        var metodo = new MetodoDePago
        {
            EstacionamientoId = TenantId,
            Nombre = n,
            Activo = true
        };
        _repository.Add(metodo);
        _repository.SaveChanges();
        return Map(metodo);
    }

    public MetodoDePagoDto Update(int id, string nuevoNombre)
    {
        var metodo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Método de pago no encontrado.");
        if (!metodo.Activo)
            throw new Exception("El método de pago está dado de baja.");

        var n = ValidarNombre(nuevoNombre);
        if (_repository.ExisteNombre(n, TenantId, id))
            throw new Exception("Ya existe un método de pago con ese nombre.");

        metodo.Nombre = n;
        _repository.SaveChanges();
        return Map(metodo);
    }

    public void Deactivate(int id)
    {
        var metodo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Método de pago no encontrado.");
        if (!metodo.Activo)
            throw new Exception("El método de pago ya está dado de baja.");

        var activos = _repository.GetAll(TenantId, includeInactivos: false);
        if (activos.Count <= 1)
            throw new Exception("Debe quedar al menos un método de pago activo.");

        metodo.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var metodo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Método de pago no encontrado.");

        if (_repository.ExisteNombre(metodo.Nombre, TenantId, id))
            throw new Exception("Ya existe un método de pago activo con ese nombre.");

        metodo.Activo = true;
        _repository.SaveChanges();
    }

    /// <summary>Alta de los 3 métodos estándar para un estacionamiento nuevo.</summary>
    public void SeedDefaults(int estacionamientoId) =>
        _repository.SeedDefaultsSiFaltan(estacionamientoId, NombresPorDefecto);

    private static string ValidarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new Exception("Debe ingresar un nombre.");
        var n = nombre.Trim();
        if (n.Length > 100)
            throw new Exception("El nombre no puede superar 100 caracteres.");
        return n;
    }

    private static MetodoDePagoDto Map(MetodoDePago m) => new()
    {
        MetodoDePagoId = m.MetodoDePagoId,
        Nombre = m.Nombre,
        Activo = m.Activo
    };
}
