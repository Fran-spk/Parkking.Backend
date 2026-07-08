using Parkking.DTOs.Abonos;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Repositories;

namespace Parkking.Services;

public class AbonoCocheraService
{
    private readonly AbonoCocheraRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public AbonoCocheraService(AbonoCocheraRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<AbonoCochera> GetAll() => _repository.GetAll(TenantId);
    public List<AbonoCochera> GetActivos() => _repository.GetActivos(TenantId);
    public List<AbonoCochera> GetByCliente(int clienteId) => _repository.GetByCliente(clienteId, TenantId);
    public List<AbonoCochera> GetByCochera(int cocheraId) => _repository.GetByCochera(cocheraId);

    public AbonoCochera GetByPatente(string patente)
    {
        var abono = _repository.GetByPatente(patente, TenantId);
        if (abono == null) throw new Exception("No se encontró ningún abono con esa patente");
        return abono;
    }

    public AbonoCochera Create(CrearAbonoRequest request)
    {
        if (_repository.GetCliente(request.ClienteId) == null)
            throw new Exception("Cliente no encontrado");
        if (_repository.GetCochera(request.CocheraId) == null)
            throw new Exception("Cochera no encontrada");

        if (!DateOnly.TryParseExact(request.FechaInicio, "yyyy-MM-dd", out var fechaInicioParsed))
            throw new Exception("El formato de FechaInicio debe ser yyyy-MM-dd");

        var fechaInicioCobroParsed = fechaInicioParsed;
        if (!string.IsNullOrEmpty(request.FechaInicioCobro))
        {
            if (!DateOnly.TryParseExact(request.FechaInicioCobro, "yyyy-MM-dd", out var parsedCobro))
                throw new Exception("El formato de FechaInicioCobro debe ser yyyy-MM-dd");
            fechaInicioCobroParsed = parsedCobro;
        }

        var nuevoAbono = new AbonoCochera
        {
            ClienteId = request.ClienteId,
            CocheraId = request.CocheraId,
            TipoVehiculoId = request.TipoVehiculoId,
            Patente = request.Patente,
            ModeloVehiculo = request.ModeloVehiculo,
            Cobrador = request.Cobrador,
            PrecioAcordado = request.PrecioAcordado,
            Activo = true,
            FechaInicio = fechaInicioParsed,
            FechaInicioCobro = fechaInicioCobroParsed
        };

        _repository.Add(nuevoAbono);
        _repository.SaveChanges();
        return nuevoAbono;
    }

    public AbonoCochera Update(int id, ModificarAbonoRequest request)
    {
        var abono = _repository.GetActivoById(id) ?? throw new Exception("Abono no encontrado");
        abono.Patente = request.Patente;
        abono.ModeloVehiculo = request.ModeloVehiculo;
        abono.Cobrador = request.Cobrador;
        abono.PrecioAcordado = request.PrecioAcordado;
        _repository.SaveChanges();
        return _repository.LoadWithIncludes(id)!;
    }

    public void MoveCochera(int id, int nuevaCocheraId)
    {
        var abono = _repository.GetActivoWithCochera(id) ?? throw new Exception("Abono no encontrado");
        var nuevaCochera = _repository.GetCocheraDestino(nuevaCocheraId, TenantId)
            ?? throw new Exception("Cochera destino no encontrada");

        if (!nuevaCochera.VehiculosPermitidos.Any(v => v.TipoVehiculoId == abono.TipoVehiculoId))
            throw new Exception("El tipo de vehículo no está permitido en la cochera destino");

        var activos = nuevaCochera.Abonos.Count(a => a.Activo);
        if (activos > 0 && !nuevaCochera.MultipleOcupacion)
            throw new Exception("La cochera destino no admite múltiples ocupaciones");

        abono.CocheraId = nuevaCocheraId;
        _repository.SaveChanges();
    }

    public void SwapCocheras(SwapCocherasRequest request)
    {
        var abono1 = _repository.GetActivoById(request.AbonoCocheraId1);
        var abono2 = _repository.GetActivoById(request.AbonoCocheraId2);
        if (abono1 == null || abono2 == null)
            throw new Exception("Uno o ambos abonos no encontrados");

        var temp = abono1.CocheraId;
        abono1.CocheraId = abono2.CocheraId;
        abono2.CocheraId = temp;
        _repository.SaveChanges();
    }

    public void Deactivate(int id)
    {
        var abono = _repository.GetActivoById(id) ?? throw new Exception("Abono no encontrado");
        abono.Activo = false;
        _repository.SaveChanges();
    }
}
