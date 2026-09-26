using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models.Finanzas;

namespace Parkking.Repositories;

public class FinanzasMovimientoRepository
{
    private readonly EstacionamientoContext _context;

    public FinanzasMovimientoRepository(EstacionamientoContext context) => _context = context;

    public List<Movimiento> GetAll(int estacionamientoId) =>
        _context.Movimientos
            .Include(m => m.Grupos)
            .Where(m => m.EstacionamientoId == estacionamientoId)
            .OrderByDescending(m => m.FechaHora)
            .ToList();

    public Movimiento? GetById(int id, int estacionamientoId) =>
        _context.Movimientos
            .Include(m => m.Grupos)
            .Include(m => m.Auditorias)
            .FirstOrDefault(m => m.MovimientoId == id && m.EstacionamientoId == estacionamientoId);

    public List<Movimiento> Filtrar(
        int estacionamientoId,
        DateTime? desde,
        DateTime? hasta,
        int? tipo,
        int? clienteId,
        int? usuarioId,
        int? grupoFinancieroId,
        int? cuentaCorrienteEstacionamientoId,
        int? cuentaCorrienteClienteId)
    {
        var query = _context.Movimientos
            .Include(m => m.Grupos)
            .Where(m => m.EstacionamientoId == estacionamientoId);

        if (desde is DateTime desdeValor)
            query = query.Where(m => m.FechaHora >= desdeValor);

        if (hasta is DateTime hastaValor)
        {
            if (hastaValor.TimeOfDay == TimeSpan.Zero)
            {
                var limite = hastaValor.Date.AddDays(1);
                query = query.Where(m => m.FechaHora < limite);
            }
            else
            {
                query = query.Where(m => m.FechaHora <= hastaValor);
            }
        }

        if (tipo is int tipoValor)
            query = query.Where(m => (int)m.Tipo == tipoValor);
        if (clienteId is int cliente)
            query = query.Where(m => m.ClienteId == cliente);
        if (usuarioId is int usuario)
            query = query.Where(m => m.UsuarioId == usuario);
        if (grupoFinancieroId is int grupo)
            query = query.Where(m => m.Grupos.Any(g => g.GrupoFinancieroId == grupo));
        if (cuentaCorrienteEstacionamientoId is int ccEst)
            query = query.Where(m => m.CuentaCorrienteEstacionamientoId == ccEst);
        if (cuentaCorrienteClienteId is int ccCli)
            query = query.Where(m => m.CuentaCorrienteClienteId == ccCli);

        return query.OrderByDescending(m => m.FechaHora).ToList();
    }

    /// <summary>
    /// Persiste el movimiento y, en el mismo agregado, su auditoría.
    /// Un solo Add y un solo SaveChanges.
    /// </summary>
    public Movimiento Registrar(
        Movimiento movimiento,
        CuentaCorrienteEstacionamiento cuentaEstacionamiento,
        CuentaCorrienteCliente? cuentaCliente,
        IEnumerable<int> grupoIds,
        string detalleAuditoria)
    {
        movimiento.Imputar(cuentaEstacionamiento, cuentaCliente);
        movimiento.AgregarGrupos(grupoIds);
        movimiento.Auditar(detalleAuditoria);

        _context.Movimientos.Add(movimiento);
        _context.SaveChanges();
        return movimiento;
    }

    public void AddCargo(Cargo cargo) => _context.Cargos.Add(cargo);

    public List<Cargo> CargosPorAbono(int abonoId, int estacionamientoId) =>
        _context.Cargos
            .Where(c => c.AbonoId == abonoId && c.EstacionamientoId == estacionamientoId)
            .OrderByDescending(c => c.FechaHora)
            .ToList();

    public List<Reintegro> ReintegrosPorAbono(int abonoId, int estacionamientoId) =>
        _context.Reintegros
            .Include(r => r.Movimiento)
            .Where(r => r.EstacionamientoId == estacionamientoId && r.Movimiento.AbonoId == abonoId)
            .OrderByDescending(r => r.FechaHora)
            .ToList();

    public void AddAjuste(AjusteFinanciero ajuste) => _context.AjustesFinancieros.Add(ajuste);

    public void AddReintegro(Reintegro reintegro) => _context.Reintegros.Add(reintegro);

    public void AddGasto(Gasto gasto) => _context.Gastos.Add(gasto);

    public bool ExisteCliente(int clienteId, int estacionamientoId) =>
        _context.Clientes.Any(c => c.ClienteId == clienteId && c.EstacionamientoId == estacionamientoId && c.Activo);

    public void SaveChanges() => _context.SaveChanges();
}
