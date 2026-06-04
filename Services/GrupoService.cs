using Microsoft.EntityFrameworkCore;
using MODELO.Contexto;
using MODELO.seguridad;

namespace Parkking_backend.Services
{
    public class GrupoService
    {
        private readonly EstacionamientoContext _context;

        public GrupoService(EstacionamientoContext context)
        {
            _context = context;
        }

        public List<Grupo> GetAll()
        {
            return _context.Grupos
                .Include(g => g.Acciones)
                .Include(g => g.Estado_Grupo)
                .ToList();
        }

        public Grupo GetById(int id)
        {
            return _context.Grupos
                .Include(g => g.Acciones)
                .Include(g => g.Estado_Grupo)
                .FirstOrDefault(g => g.GRU_ID == id);
        }

        public List<Grupo> GetByEstado(Estado_Grupo estado)
        {
            return _context.Grupos
                .Include(g => g.Acciones)
                .Include(g => g.Estado_Grupo)
                .Where(x => x.EST_GRU_ID == estado.EST_GRU_ID)
                .ToList();
        }

        public List<Estado_Grupo> GetAllEstados()
        {
            return _context.Estados_Grupos
                .ToList();
        }

        public List<Modulo> GetAllModulos()
        {
            return _context.Modulos
                .Include(m => m.Formularios)
                    .ThenInclude(f => f.Acciones)
                .ToList();
        }

        public Grupo Create(Grupo grupo)
        {
            var grupoExistente = _context.Grupos
                .FirstOrDefault(x => x.GRU_NOMBRE == grupo.GRU_NOMBRE);

            if (grupoExistente != null)
                throw new Exception("Ya existe el grupo " + grupo.GRU_NOMBRE + " en el sistema");

            _context.Grupos.Add(grupo);
            _context.SaveChanges();
            return grupo;
        }

        public void Delete(Grupo grupo)
        {
            var grupoExistente = _context.Grupos
                .FirstOrDefault(x => x.GRU_ID == grupo.GRU_ID);

            if (grupoExistente == null)
                throw new Exception("El grupo " + grupo.GRU_NOMBRE + " no existe");

            _context.Grupos.Remove(grupoExistente);
            _context.SaveChanges();
        }

        public void Update(Grupo grupo)
        {
            var grupoExistente = _context.Grupos
                .FirstOrDefault(x => x.GRU_ID == grupo.GRU_ID);

            if (grupoExistente == null)
                throw new Exception("El grupo " + grupo.GRU_NOMBRE + " no existe");

            grupoExistente.GRU_NOMBRE = grupo.GRU_NOMBRE;
            grupoExistente.EST_GRU_ID = grupo.EST_GRU_ID;

            _context.SaveChanges();
        }
    }
}
