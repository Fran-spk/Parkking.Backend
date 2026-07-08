using Parkking.DTOs.Tarifas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models;
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

    public List<TarifaVigenteDto> GetVigentes()
    {
        return _repository.GetAll(TenantId)
            .GroupBy(t => new { t.TipoVehiculoId, t.CategoriaCocheraId })
            .Select(g =>
            {
                var t = g.OrderByDescending(x => x.FechaHoraActualizacion).First();
                return new TarifaVigenteDto
                {
                    TarifaMensualId = t.TarifaMensualId,
                    TipoVehiculoId = t.TipoVehiculoId,
                    CategoriaCocheraId = t.CategoriaCocheraId,
                    Precio = t.Precio,
                    FechaActualizacion = t.FechaHoraActualizacion
                };
            }).ToList();
    }

    public TarifaVigenteDto GetVigente(int tipoVehiculoId, int categoriaCocheraId)
    {
        var tarifa = _repository.GetByCombinacion(tipoVehiculoId, categoriaCocheraId, TenantId).FirstOrDefault();
        if (tarifa == null) throw new Exception("No hay tarifa vigente para esta combinación");

        return new TarifaVigenteDto
        {
            TarifaMensualId = tarifa.TarifaMensualId,
            TipoVehiculoId = tarifa.TipoVehiculoId,
            CategoriaCocheraId = tarifa.CategoriaCocheraId,
            Precio = tarifa.Precio,
            FechaActualizacion = tarifa.FechaHoraActualizacion
        };
    }

    public List<TarifaHistorialDto> GetHistorial(int tipoVehiculoId, int categoriaCocheraId) =>
        _repository.GetByCombinacion(tipoVehiculoId, categoriaCocheraId, TenantId)
            .Select(t => new TarifaHistorialDto
            {
                TarifaMensualId = t.TarifaMensualId,
                Precio = t.Precio,
                FechaActualizacion = t.FechaHoraActualizacion
            }).ToList();

    public List<TarifaMensual> GetAll() => _repository.GetAll(TenantId);

    public TarifaMensual Create(CrearTarifaRequest request)
    {
        var tarifa = new TarifaMensual
        {
            EstacionamientoId = TenantId,
            TipoVehiculoId = request.TipoVehiculoId,
            CategoriaCocheraId = request.CategoriaCocheraId,
            Precio = request.Precio,
            FechaHoraActualizacion = DateTime.UtcNow
        };
        _repository.Add(tarifa);
        _repository.SaveChanges();
        return tarifa;
    }
}
