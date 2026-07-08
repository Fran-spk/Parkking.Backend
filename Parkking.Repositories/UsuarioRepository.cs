using Microsoft.EntityFrameworkCore;
using Parkking.Infrastructure.Persistence;
using Parkking.Models;
using Parkking.Models.Seguridad;

namespace Parkking.Repositories;

public class UsuarioRepository
{
    private readonly EstacionamientoContext _context;

    public UsuarioRepository(EstacionamientoContext context) => _context = context;

    public Usuario? GetByLogin(string login) =>
        _context.Usuarios
            .FirstOrDefault(u => u.USU_USUARIO == login || u.USU_MAIL == login);

    public List<Estacionamiento> GetEstacionamientosUsuario(int usuarioId) =>
        _context.UsuarioEstacionamientos
            .Include(x => x.Estacionamiento)
            .Where(x => x.USU_ID == usuarioId && x.Activo)
            .Select(x => x.Estacionamiento)
            .ToList();

    public bool TieneAccesoEstacionamiento(int usuarioId, int estacionamientoId) =>
        _context.UsuarioEstacionamientos.Any(x =>
            x.USU_ID == usuarioId && x.ESTACIONAMIENTO_ID == estacionamientoId && x.Activo);
}
