using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

public class GrupoFinancieroService
{
    private readonly GrupoFinancieroRepository _repository;
    private readonly ReglaAsignacionRepository _reglas;
    private readonly FinanzasMovimientoRepository _movimientos;
    private readonly IEstacionamientoContext _estacionamiento;

    public GrupoFinancieroService(
        GrupoFinancieroRepository repository,
        ReglaAsignacionRepository reglas,
        FinanzasMovimientoRepository movimientos,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _reglas = reglas;
        _movimientos = movimientos;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public List<GrupoFinancieroDto> GetAll(bool includeInactivos = false) =>
        _repository.GetAll(TenantId, includeInactivos).Select(Map).ToList();

    public GrupoFinancieroDto? GetById(int id)
    {
        var grupo = _repository.GetById(id, TenantId);
        return grupo == null ? null : Map(grupo);
    }

    public GrupoFinancieroPeriodoDto ConsultarPeriodo(int id, DateTime? desde, DateTime? hasta)
    {
        var grupo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Grupo financiero no encontrado.");

        var movimientos = _movimientos.Filtrar(TenantId, desde, hasta, null, null, null, id, null, null)
            .Select(m => new MovimientoDto
            {
                MovimientoId = m.MovimientoId,
                EstacionamientoId = m.EstacionamientoId,
                CuentaCorrienteEstacionamientoId = m.CuentaCorrienteEstacionamientoId,
                ClienteId = m.ClienteId,
                CuentaCorrienteClienteId = m.CuentaCorrienteClienteId,
                PagoId = m.PagoId,
                Importe = m.Importe,
                Tipo = (int)m.Tipo,
                TipoDescripcion = m.Tipo.ToString(),
                Concepto = m.Concepto,
                FechaHora = m.FechaHora,
                UsuarioId = m.UsuarioId,
                GrupoFinancieroIds = m.Grupos.Select(g => g.GrupoFinancieroId).ToList()
            })
            .ToList();

        var ingresos = movimientos.Where(m => m.Importe > 0).Sum(m => m.Importe);
        var egresos = movimientos.Where(m => m.Importe < 0).Sum(m => -m.Importe);

        return new GrupoFinancieroPeriodoDto
        {
            GrupoFinancieroId = grupo.GrupoFinancieroId,
            Nombre = grupo.Nombre,
            Desde = desde,
            Hasta = hasta,
            TotalIngresos = ingresos,
            TotalEgresos = egresos,
            Neto = ingresos - egresos,
            Movimientos = movimientos
        };
    }

    public GrupoFinancieroDto Create(CrearGrupoFinancieroRequest request)
    {
        var nombre = ValidarNombre(request.Nombre);
        var descripcion = NormalizarDescripcion(request.Descripcion);
        if (_repository.ExisteNombreActivo(nombre, TenantId))
            throw new Exception("Ya existe un grupo financiero activo con ese nombre.");

        var grupo = new GrupoFinanciero
        {
            EstacionamientoId = TenantId,
            Nombre = nombre,
            Descripcion = descripcion,
            Activo = true
        };
        _repository.Add(grupo);
        _repository.SaveChanges();
        return Map(grupo);
    }

    public GrupoFinancieroDto Update(int id, EditarGrupoFinancieroRequest request)
    {
        var grupo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Grupo financiero no encontrado.");

        var nombre = ValidarNombre(request.Nombre);
        if (_repository.ExisteNombreActivo(nombre, TenantId, id))
            throw new Exception("Ya existe un grupo financiero activo con ese nombre.");

        grupo.Nombre = nombre;
        grupo.Descripcion = NormalizarDescripcion(request.Descripcion);
        _repository.SaveChanges();
        return Map(grupo);
    }

    public void Deactivate(int id)
    {
        var grupo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Grupo financiero no encontrado.");
        if (!grupo.Activo)
            throw new Exception("El grupo financiero ya está dado de baja.");

        grupo.Activo = false;
        _repository.SaveChanges();
    }

    public void Reactivate(int id)
    {
        var grupo = _repository.GetById(id, TenantId)
            ?? throw new Exception("Grupo financiero no encontrado.");
        if (grupo.Activo)
            throw new Exception("El grupo financiero ya está activo.");
        if (_repository.ExisteNombreActivo(grupo.Nombre, TenantId, id))
            throw new Exception("Ya existe un grupo financiero activo con ese nombre.");

        grupo.Activo = true;
        _repository.SaveChanges();
    }

    /// <summary>
    /// Si hay reglas activas para el cliente o el tipo de gasto del movimiento,
    /// crea los MovimientoGrupoFinanciero sobre ese movimiento.
    /// </summary>
    public void AsignarGrupos(Movimiento movimiento)
    {
        var grupoIds = _reglas.GruposQueMatchean(TenantId, movimiento.ClienteId, movimiento.TipoGastoId);
        if (grupoIds.Count == 0) return;
        movimiento.AgregarGrupos(grupoIds);
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

    private static string? NormalizarDescripcion(string? descripcion)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) return null;
        var d = descripcion.Trim();
        if (d.Length > 255)
            throw new Exception("La descripción no puede superar 255 caracteres.");
        return d;
    }

    private static GrupoFinancieroDto Map(GrupoFinanciero g) => new()
    {
        GrupoFinancieroId = g.GrupoFinancieroId,
        Nombre = g.Nombre,
        Descripcion = g.Descripcion,
        Activo = g.Activo
    };
}
