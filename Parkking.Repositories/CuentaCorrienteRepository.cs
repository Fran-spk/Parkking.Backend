using Parkking.Infrastructure.Persistence;
using Parkking.Models.Finanzas;

namespace Parkking.Repositories;

public class CuentaCorrienteRepository
{
    private readonly EstacionamientoContext _context;

    public CuentaCorrienteRepository(EstacionamientoContext context) => _context = context;

    public CuentaCorrienteEstacionamiento? GetEstacionamiento(int estacionamientoId) =>
        _context.CuentasCorrientesEstacionamiento
            .FirstOrDefault(c => c.EstacionamientoId == estacionamientoId);

    public CuentaCorrienteCliente? GetCliente(int clienteId, int estacionamientoId) =>
        _context.CuentasCorrientesCliente
            .FirstOrDefault(c => c.ClienteId == clienteId && c.EstacionamientoId == estacionamientoId);

    public void Add(CuentaCorrienteEstacionamiento cuenta) =>
        _context.CuentasCorrientesEstacionamiento.Add(cuenta);

    public void Add(CuentaCorrienteCliente cuenta) =>
        _context.CuentasCorrientesCliente.Add(cuenta);

    public void SaveChanges() => _context.SaveChanges();
}
