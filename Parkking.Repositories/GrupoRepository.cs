using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models.Enums;
using Parkking.Models.Seguridad;

namespace Parkking.Repositories;

public class GrupoRepository
{
    private readonly EstacionamientoContext _context;

    public GrupoRepository(EstacionamientoContext context) => _context = context;

    public List<Grupo> GetAll() =>
        _context.Grupos.Include(g => g.Acciones).Include(g => g.EstadoGrupo).ToList();

    public Grupo? GetById(int id) =>
        _context.Grupos.Include(g => g.Acciones).Include(g => g.EstadoGrupo)
            .FirstOrDefault(g => g.GRU_ID == id);

    public List<Grupo> GetByEstado(EstadoGrupo estado) =>
        _context.Grupos.Include(g => g.Acciones).Include(g => g.EstadoGrupo)
            .Where(x => x.EstadoGrupo == estado).ToList();



    public List<Modulo> GetAllModulos() =>
        _context.Modulos.ToList();

    public Grupo? FindByNombre(string nombre) =>
        _context.Grupos.FirstOrDefault(x => x.GRU_NOMBRE == nombre);

    public Grupo? FindById(int id) => _context.Grupos.FirstOrDefault(x => x.GRU_ID == id);

    public void Add(Grupo grupo) => _context.Grupos.Add(grupo);
    public void Remove(Grupo grupo) => _context.Grupos.Remove(grupo);
    public void SaveChanges() => _context.SaveChanges();
}
