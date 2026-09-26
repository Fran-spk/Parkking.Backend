using Parkking.DTOs.Finanzas;
using Parkking.Infrastructure.Tenant;
using Parkking.Models.Finanzas;
using Parkking.Repositories;

namespace Parkking.Services;

/// <summary>
/// Busca la cuenta corriente del tenant o del cliente. Si no existe, la crea.
/// </summary>
public class CuentaCorrienteService
{
    private readonly CuentaCorrienteRepository _repository;
    private readonly FinanzasMovimientoRepository _movimientos;
    private readonly IEstacionamientoContext _estacionamiento;

    public CuentaCorrienteService(
        CuentaCorrienteRepository repository,
        FinanzasMovimientoRepository movimientos,
        IEstacionamientoContext estacionamiento)
    {
        _repository = repository;
        _movimientos = movimientos;
        _estacionamiento = estacionamiento;
    }

    private int TenantId => _estacionamiento.EstacionamientoId;

    public CuentaCorrienteEstacionamiento AsegurarEstacionamiento()
    {
        var existente = _repository.GetEstacionamiento(TenantId);
        if (existente != null) return existente;

        var cuenta = CuentaCorrienteEstacionamiento.Abrir(TenantId);
        _repository.Add(cuenta);
        _repository.SaveChanges();
        return cuenta;
    }

    public CuentaCorrienteCliente AsegurarCliente(int clienteId)
    {
        var existente = _repository.GetCliente(clienteId, TenantId);
        if (existente != null) return existente;

        var cuenta = CuentaCorrienteCliente.Abrir(TenantId, clienteId);
        _repository.Add(cuenta);
        _repository.SaveChanges();
        return cuenta;
    }

    public CuentaCorrienteConsultaDto ConsultarEstacionamiento(DateTime? desde, DateTime? hasta)
    {
        var cuenta = _repository.GetEstacionamiento(TenantId);
        if (cuenta == null)
        {
            return new CuentaCorrienteConsultaDto
            {
                EstacionamientoId = TenantId,
                Saldo = 0,
                Existe = false
            };
        }

        return new CuentaCorrienteConsultaDto
        {
            CuentaCorrienteId = cuenta.CuentaCorrienteEstacionamientoId,
            EstacionamientoId = TenantId,
            Saldo = cuenta.Saldo,
            Existe = true,
            Movimientos = Map(_movimientos.Filtrar(
                TenantId, desde, hasta, null, null, null, null, cuenta.CuentaCorrienteEstacionamientoId, null))
        };
    }

    public CuentaCorrienteConsultaDto ConsultarCliente(int clienteId, DateTime? desde, DateTime? hasta)
    {
        if (!_movimientos.ExisteCliente(clienteId, TenantId))
            throw new Exception("El cliente no existe o está dado de baja.");

        var cuenta = _repository.GetCliente(clienteId, TenantId);
        if (cuenta == null)
        {
            return new CuentaCorrienteConsultaDto
            {
                EstacionamientoId = TenantId,
                ClienteId = clienteId,
                Saldo = 0,
                Existe = false
            };
        }

        return new CuentaCorrienteConsultaDto
        {
            CuentaCorrienteId = cuenta.CuentaCorrienteClienteId,
            EstacionamientoId = TenantId,
            ClienteId = clienteId,
            Saldo = cuenta.Saldo,
            Existe = true,
            Movimientos = Map(_movimientos.Filtrar(
                TenantId, desde, hasta, null, clienteId, null, null, null, cuenta.CuentaCorrienteClienteId))
        };
    }

    private static List<MovimientoDto> Map(IEnumerable<Movimiento> movimientos) =>
        movimientos.Select(m => new MovimientoDto
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
        }).ToList();
}
