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

    public Estacionamiento? GetById(int id)
    {
        return _context.Estacionamientos
            .FirstOrDefault(e => e.EstacionamientoId == id);
    }

    public void Add(Estacionamiento estacionamiento)
    {
        _context.Estacionamientos.Add(estacionamiento);
        _context.SaveChanges();
    }

    public void Update(Estacionamiento estacionamiento)
    {
        _context.Estacionamientos.Update(estacionamiento);
        _context.SaveChanges();
    }
}