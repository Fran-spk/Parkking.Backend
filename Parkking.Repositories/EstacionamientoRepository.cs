using Parkking.Infrastructure.Persistence;
using Parkking.Models;

namespace Parkking.Repositories;

public class EstacionamientoRepository
{
    private readonly EstacionamientoContext _context;

    public EstacionamientoRepository(EstacionamientoContext context)
    {
        _context = context;
    }

    public DatosEstacionamiento? GetById(int id) =>
        _context.Estacionamientos.FirstOrDefault(e => e.EstacionamientoId == id);

    public void Add(DatosEstacionamiento datos)
    {
        _context.Estacionamientos.Add(datos);
        _context.SaveChanges();
    }

    public void Update(DatosEstacionamiento datos)
    {
        _context.Estacionamientos.Update(datos);
        _context.SaveChanges();
    }
}
