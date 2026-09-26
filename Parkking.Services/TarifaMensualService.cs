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

    /// <summary>
    /// Crea tarifas faltantes tipo × categoría × todas las periodicidades.
    /// Base = tarifa mensual vigente; si no hay, <paramref name="precioMensualDefault"/>.
    /// Quincenal = base/2; bi/tri/sem/anual = base × N meses.
    /// </summary>
    public int CompletarFaltantes(decimal precioMensualDefault = 50_000m)
    {
        if (precioMensualDefault <= 0)
            throw new Exception("El precio mensual por defecto debe ser mayor a cero");

        var factores = new Dictionary<PeriodicidadCobro, decimal>
        {
            [PeriodicidadCobro.Mensual] = 1m,
            [PeriodicidadCobro.Quincenal] = 0.5m,
            [PeriodicidadCobro.Bimestral] = 2m,
            [PeriodicidadCobro.Trimestral] = 3m,
            [PeriodicidadCobro.Semestral] = 6m,
            [PeriodicidadCobro.Anual] = 12m,
        };

        var tipos = _repository.GetTipoIdsActivos(TenantId);
        var cats = _repository.GetCategoriaIdsActivas(TenantId);
        if (tipos.Count == 0 || cats.Count == 0)
            return 0;

        var ahora = DateTime.UtcNow;
        var creadas = 0;

        foreach (var tipoId in tipos)
        foreach (var catId in cats)
        {
            var baseMensual = _repository.PrecioMensualVigente(TenantId, tipoId, catId)
                ?? precioMensualDefault;

            foreach (var (periodicidad, factor) in factores)
            {
                if (_repository.ExisteCombinacion(TenantId, tipoId, catId, periodicidad))
                    continue;

                var precio = Math.Round(baseMensual * factor, 0, MidpointRounding.AwayFromZero);
                if (precio < 1) precio = 1;

                _repository.Add(new TarifaMensual
                {
                    EstacionamientoId = TenantId,
                    TipoVehiculoId = tipoId,
                    CategoriaCocheraId = catId,
                    PeriodicidadCobro = periodicidad,
                    Precio = precio,
                    FechaHoraActualizacion = ahora,
                });
                creadas++;
            }
        }

        if (creadas > 0)
            _repository.SaveChanges();

        return creadas;
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
