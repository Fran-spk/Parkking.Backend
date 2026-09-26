using Parkking.DTOs.Finanzas;
using Parkking.Models.Enums;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

/// <summary>
/// Los movimientos se crean desde pagos, ajustes y reintegros.
/// Este servicio los registra y los lista. No tiene alta propia.
/// </summary>
public class MovimientoService
{
    private readonly FinanzasMovimientoRepository _repository;
    private readonly CuentaCorrienteService _cuentas;
    private readonly GrupoFinancieroService _grupos;

    public MovimientoService(
        FinanzasMovimientoRepository repository,
        CuentaCorrienteService cuentas,
        GrupoFinancieroService grupos)
    {
        _repository = repository;
        _cuentas = cuentas;
        _grupos = grupos;
    }

    public List<MovimientoDto> GetAll(int estacionamientoId) =>
        GetFiltrados(estacionamientoId, null, null, null, null, null, null);

    public List<MovimientoDto> GetFiltrados(
        int estacionamientoId,
        DateTime? desde,
        DateTime? hasta,
        int? tipo,
        int? clienteId,
        int? usuarioId,
        int? grupoFinancieroId) =>
        _repository.Filtrar(estacionamientoId, desde, hasta, tipo, clienteId, usuarioId, grupoFinancieroId, null, null)
            .Select(Map)
            .ToList();

    public MovimientoDetalleDto? GetById(int id, int estacionamientoId)
    {
        var movimiento = _repository.GetById(id, estacionamientoId);
        if (movimiento == null) return null;
        var dto = new MovimientoDetalleDto();
        Copiar(movimiento, dto);
        dto.Auditorias = movimiento.Auditorias
            .OrderByDescending(a => a.FechaHora)
            .Select(a => new AuditoriaMovimientoDto
            {
                AuditoriaMovimientoId = a.AuditoriaMovimientoId,
                FechaHora = a.FechaHora,
                UsuarioId = a.UsuarioId,
                Ip = a.Ip,
                UserAgent = a.UserAgent,
                Detalle = a.Detalle
            })
            .ToList();
        return dto;
    }

    public Movimiento Registrar(Movimiento movimiento, IEnumerable<int> grupoIds, string detalleAuditoria)
    {
        var cuentaEstacionamiento = _cuentas.AsegurarEstacionamiento();
        movimiento.CuentaCorrienteEstacionamientoId = cuentaEstacionamiento.CuentaCorrienteEstacionamientoId;

        CuentaCorrienteCliente? cuentaCliente = null;
        if (movimiento.ClienteId is int clienteId)
        {
            cuentaCliente = _cuentas.AsegurarCliente(clienteId);
            movimiento.CuentaCorrienteClienteId = cuentaCliente.CuentaCorrienteClienteId;
        }

        _grupos.AsignarGrupos(movimiento);
        _repository.Registrar(movimiento, cuentaEstacionamiento, cuentaCliente, grupoIds, detalleAuditoria);
        return movimiento;
    }

    private static MovimientoDto Map(Movimiento m)
    {
        var dto = new MovimientoDto();
        Copiar(m, dto);
        return dto;
    }

    private static void Copiar(Movimiento m, MovimientoDto dto)
    {
        dto.MovimientoId = m.MovimientoId;
        dto.EstacionamientoId = m.EstacionamientoId;
        dto.CuentaCorrienteEstacionamientoId = m.CuentaCorrienteEstacionamientoId;
        dto.ClienteId = m.ClienteId;
        dto.CuentaCorrienteClienteId = m.CuentaCorrienteClienteId;
        dto.PagoId = m.PagoId;
        dto.Importe = m.Importe;
        dto.Tipo = (int)m.Tipo;
        dto.TipoDescripcion = m.Tipo.ToString();
        dto.Concepto = m.Concepto;
        dto.FechaHora = m.FechaHora;
        dto.UsuarioId = m.UsuarioId;
        dto.GrupoFinancieroIds = m.Grupos.Select(g => g.GrupoFinancieroId).ToList();
    }
}
