using Parkking.DTOs.Tarifas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
using Parkking.Models.Enums;
using Parkking.Repositories;

namespace Parkking.Services;

public class TarifaMensualService
{
    private readonly TarifaMensualRepository _repository;
    private readonly IEstacionamientoContext _estacionamiento;

    public TarifaMensualService(TarifaMensualRepository repository, IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<TarifaVigenteDto> GetVigentes(PeriodicidadCobro? periodicidadCobro = null)
    {
        var todas = _repository.GetAll(TenantId).AsEnumerable();
        if (periodicidadCobro.HasValue)
            todas = todas.Where(t => t.PeriodicidadCobro == periodicidadCobro.Value);

        return todas
            .GroupBy(t => new { t.TipoVehiculoId, t.CategoriaCocheraId, t.PeriodicidadCobro })
            .Select(g =>
            {
                var t = g.OrderByDescending(x => x.FechaHoraActualizacion).First();
                return ToVigenteDto(t);
            }).ToList();
    }

    public TarifaVigenteDto GetVigente(int tipoVehiculoId, int categoriaCocheraId, PeriodicidadCobro periodicidadCobro)
    {
        var tarifa = _repository
            .GetByCombinacion(tipoVehiculoId, categoriaCocheraId, periodicidadCobro, TenantId)
            .FirstOrDefault()
            ?? throw new Exception("No hay tarifa vigente para esta combinación de tipo, categoría y periodicidad");

        return ToVigenteDto(tarifa);
    }

    public List<TarifaHistorialDto> GetHistorial(
        int tipoVehiculoId,
        int categoriaCocheraId,
        PeriodicidadCobro periodicidadCobro) =>
        _repository.GetByCombinacion(tipoVehiculoId, categoriaCocheraId, periodicidadCobro, TenantId)
            .Select(t => new TarifaHistorialDto
            {
                TarifaMensualId = t.TarifaMensualId,
                Precio = t.Precio,
                FechaActualizacion = t.FechaHoraActualizacion
            }).ToList();

    public List<TarifaMensual> GetAll() => _repository.GetAll(TenantId);

    public TarifaMensual Create(CrearTarifaRequest request)
    {
        if (!Enum.IsDefined(typeof(PeriodicidadCobro), request.PeriodicidadCobro))
            throw new Exception("Periodicidad de cobro inválida");
        if (request.Precio <= 0)
            throw new Exception("El precio debe ser mayor a cero");

        var tarifa = new TarifaMensual
        {
            EstacionamientoId = TenantId,
            TipoVehiculoId = request.TipoVehiculoId,
            CategoriaCocheraId = request.CategoriaCocheraId,
            PeriodicidadCobro = request.PeriodicidadCobro,
            Precio = request.Precio,
            FechaHoraActualizacion = DateTime.UtcNow
        };
        _repository.Add(tarifa);
        _repository.SaveChanges();
        return tarifa;
    }

    public static TarifaVigenteDto ToVigenteDto(TarifaMensual t) => new()
    {
        TarifaMensualId = t.TarifaMensualId,
        TipoVehiculoId = t.TipoVehiculoId,
        CategoriaCocheraId = t.CategoriaCocheraId,
        PeriodicidadCobro = t.PeriodicidadCobro,
        Precio = t.Precio,
        FechaActualizacion = t.FechaHoraActualizacion
    };
}
