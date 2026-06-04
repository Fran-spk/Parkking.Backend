using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using static Parkking_backend.Controllers.API.Controllers.CategoriaCocheraController;

namespace Parkking_backend.Services
{
    public class CategoriaService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public CategoriaService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public List<CategoriaCochera> GetAll(bool includeInactivos = false)
        {
            var query = _context.CategoriasCochera
                .Where(c => c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (!includeInactivos)
                query = query.Where(c => c.Activo);

            return query.OrderBy(c => c.Nombre).ToList();
        }

        public CategoriaCochera GetById(int id)
        {
            return _context.CategoriasCochera
                .FirstOrDefault(c => c.CategoriaCocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId);
        }

        public CategoriaCochera Create(CategoriaCocheraRequest request)
        {
            var existe = _context.CategoriasCochera
                .Any(c => c.Nombre == request.Nombre
                       && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                       && c.Activo);

            if (existe)
                throw new Exception("Ya existe una categoría con ese nombre");

            var categoria = new CategoriaCochera
            {
                EstacionamientoId = _estacionamiento.EstacionamientoId,
                Nombre = request.Nombre,
                Activo = true
            };

            _context.CategoriasCochera.Add(categoria);
            _context.SaveChanges();
            return categoria;
        }

        public CategoriaCochera Update(int id, CategoriaCocheraRequest request)
        {
            var categoria = _context.CategoriasCochera
                .FirstOrDefault(c => c.CategoriaCocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            if (!categoria.Activo)
                throw new Exception("La categoría está dada de baja");

            var duplicado = _context.CategoriasCochera
                .Any(c => c.Nombre == request.Nombre
                       && c.CategoriaCocheraId != id
                       && c.EstacionamientoId == _estacionamiento.EstacionamientoId
                       && c.Activo);

            if (duplicado)
                throw new Exception("Ya existe una categoría con ese nombre");

            categoria.Nombre = request.Nombre;
            _context.SaveChanges();
            return categoria;
        }

        public void Deactivate(int id)
        {
            var categoria = _context.CategoriasCochera
                .FirstOrDefault(c => c.CategoriaCocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            if (!categoria.Activo)
                throw new Exception("La categoría ya está dada de baja");

            var tieneCocheras = _context.Cocheras
                .Any(c => c.CategoriaCocheraId == id && c.Activo);

            if (tieneCocheras)
                throw new Exception("No se puede dar de baja, hay cocheras activas con esta categoría");

            categoria.Activo = false;
            _context.SaveChanges();
        }

        public void Reactivate(int id)
        {
            var categoria = _context.CategoriasCochera
                .FirstOrDefault(c => c.CategoriaCocheraId == id
                                   && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (categoria == null)
                throw new Exception("Categoría no encontrada");

            categoria.Activo = true;
            _context.SaveChanges();
        }
    }
}
