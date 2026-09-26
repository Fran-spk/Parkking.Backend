using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class DocumentoRepository
{
    private readonly EstacionamientoContext _context;

    public DocumentoRepository(EstacionamientoContext context) => _context = context;

    public Documento? GetById(int id) =>
        _context.Documentos
            .Include(d => d.Cliente)
            .Include(d => d.Vehiculo)
            .Include(d => d.Abono)
            .FirstOrDefault(d => d.DocumentoId == id && d.Activo);

    public void Add(Documento doc) => _context.Documentos.Add(doc);

    public void SaveChanges() => _context.SaveChanges();

    public List<Documento> ListActivosByCliente(int clienteId) =>
        _context.Documentos
            .Include(d => d.Cliente)
            .Where(d => d.Activo && d.ClienteId == clienteId)
            .OrderByDescending(d => d.FechaCarga)
            .ToList();

    public List<Documento> ListActivosByVehiculos(IEnumerable<int> vehiculoIds)
    {
        var ids = vehiculoIds.Distinct().ToList();
        if (ids.Count == 0) return new List<Documento>();

        return _context.Documentos
            .Include(d => d.Vehiculo)
            .Where(d => d.Activo && d.VehiculoId != null && ids.Contains(d.VehiculoId.Value))
            .OrderByDescending(d => d.FechaCarga)
            .ToList();
    }

    public List<Documento> ListActivosByAbono(int abonoId) =>
        _context.Documentos
            .Include(d => d.Abono)
            .Where(d => d.Activo && d.AbonoId == abonoId)
            .OrderByDescending(d => d.FechaCarga)
            .ToList();

    public Abono? GetAbonoWithVehiculos(int abonoId) =>
        _context.Abonos
            .Include(a => a.Cliente)
            .Include(a => a.AbonoVehiculos).ThenInclude(av => av.Vehiculo)
            .FirstOrDefault(a => a.AbonoId == abonoId);

    public Cliente? GetCliente(int clienteId) =>
        _context.Clientes.FirstOrDefault(c => c.ClienteId == clienteId);

    public Vehiculo? GetVehiculo(int vehiculoId) =>
        _context.Vehiculos.FirstOrDefault(v => v.VehiculoId == vehiculoId);
}
