using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Enums;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

public class ReglaAsignacionService
{
    private readonly ReglaAsignacionRepository _repository;
    private readonly GrupoFinancieroRepository _grupos;
    private readonly IEstacionamientoContext _estacionamiento;

    public ReglaAsignacionService(
        ReglaAsignacionRepository repository,
        GrupoFinancieroRepository grupos,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _grupos = grupos;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<ReglaAsignacionDto> GetAll(bool includeInactivas = false) =>
        _repository.GetAll(TenantId, includeInactivas).Select(Map).ToList();

    public ReglaAsignacionDto? GetById(int id)
    {
        var regla = _repository.GetById(id, TenantId);
        return regla == null ? null : Map(regla);
    }

    public ReglaAsignacionDto Create(CrearReglaAsignacionRequest request)
    {
        var (criterio, clienteId, tipoGastoId) = ValidarCriterio(request.Criterio, request.ClienteId, request.TipoGastoId);
        var grupo = RequireGrupoActivo(request.GrupoFinancieroId);

        var regla = new ReglaAsignacion
        {
            EstacionamientoId = TenantId,
            GrupoFinancieroId = grupo.GrupoFinancieroId,
            GrupoFinanciero = grupo,
            Criterio = criterio,
            ClienteId = clienteId,
            TipoGastoId = tipoGastoId,
            Activa = true
        };
        _repository.Add(regla);
        _repository.SaveChanges();
        return Map(regla);
    }

    public ReglaAsignacionDto Update(int id, EditarReglaAsignacionRequest request)
    {
        var regla = _repository.GetById(id, TenantId)
            ?? throw new Exception("Regla de asignación no encontrada.");

        var (criterio, clienteId, tipoGastoId) = ValidarCriterio(request.Criterio, request.ClienteId, request.TipoGastoId);
        var grupo = RequireGrupoActivo(request.GrupoFinancieroId);

        regla.GrupoFinancieroId = grupo.GrupoFinancieroId;
        regla.GrupoFinanciero = grupo;
        regla.Criterio = criterio;
        regla.ClienteId = clienteId;
        regla.TipoGastoId = tipoGastoId;
        _repository.SaveChanges();
        return Map(regla);
    }

    public void Deactivate(int id)
    {
        var regla = _repository.GetById(id, TenantId)
            ?? throw new Exception("Regla de asignación no encontrada.");
        if (!regla.Activa)
            throw new Exception("La regla ya está dada de baja.");

        regla.Activa = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var regla = _repository.GetById(id, TenantId)
            ?? throw new Exception("Regla de asignación no encontrada.");
        if (regla.Activa)
            throw new Exception("La regla ya está activa.");

        RequireGrupoActivo(regla.GrupoFinancieroId);
        regla.Activa = true;
        _repository.SaveChanges();
    }

    private GrupoFinanciero RequireGrupoActivo(int grupoFinancieroId)
    {
        var grupo = _grupos.GetById(grupoFinancieroId, TenantId)
            ?? throw new Exception("Grupo financiero no encontrado.");
        if (!grupo.Activo)
            throw new Exception("El grupo financiero está dado de baja.");
        return grupo;
    }

    private (CriterioRegla Criterio, int? ClienteId, int? TipoGastoId) ValidarCriterio(
        int criterio,
        int? clienteId,
        int? tipoGastoId)
    {
        if (!Enum.IsDefined(typeof(CriterioRegla), criterio))
            throw new Exception("El criterio de la regla no es válido.");

        var c = (CriterioRegla)criterio;
        if (c == CriterioRegla.Cliente)
        {
            if (clienteId is not > 0)
                throw new Exception("Debe indicar el cliente de la regla.");
            if (tipoGastoId is > 0)
                throw new Exception("Una regla de cliente no lleva tipo de gasto.");
            if (!_repository.ExisteCliente(clienteId.Value, TenantId))
                throw new Exception("El cliente no existe o está dado de baja.");
            return (c, clienteId, null);
        }

        if (tipoGastoId is not > 0)
            throw new Exception("Debe indicar el tipo de gasto de la regla.");
        if (clienteId is > 0)
            throw new Exception("Una regla de tipo de gasto no lleva cliente.");
        if (!_repository.ExisteTipoGasto(tipoGastoId.Value, TenantId))
            throw new Exception("El tipo de gasto no existe o está dado de baja.");
        return (c, null, tipoGastoId);
    }

    private static ReglaAsignacionDto Map(ReglaAsignacion r) => new()
    {
        ReglaAsignacionId = r.ReglaAsignacionId,
        GrupoFinancieroId = r.GrupoFinancieroId,
        GrupoNombre = r.GrupoFinanciero?.Nombre ?? string.Empty,
        Criterio = (int)r.Criterio,
        CriterioDescripcion = r.Criterio == CriterioRegla.Cliente ? "Cliente" : "Tipo de gasto",
        ClienteId = r.ClienteId,
        TipoGastoId = r.TipoGastoId,
        Activa = r.Activa
    };
}
