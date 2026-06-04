using MODELO;
using MODELO.Contexto;
using Parkking_backend.Models;
using Parkking_backend.Services;

namespace Parkking_backend.Services
{
    public class EstacionamientoService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public EstacionamientoService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public Estacionamiento Get()
        {
            var estacionamiento = _context.Estacionamientos
                .FirstOrDefault(e => e.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (estacionamiento == null)
                throw new Exception("Estacionamiento no encontrado");

            return estacionamiento;
        }

        public Estacionamiento GetById(int id)
        {
            return _context.Estacionamientos
                .FirstOrDefault(e => e.EstacionamientoId == id);
        }

        public Estacionamiento Update(ModificarEstacionamientoRequest request)
        {
            var estacionamiento = _context.Estacionamientos
                .FirstOrDefault(e => e.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (estacionamiento == null)
                throw new Exception("Estacionamiento no encontrado");

            estacionamiento.DiaVencimientoAbono = request.DiaVencimientoAbono;
            estacionamiento.AplicaRecargo = request.AplicaRecargo;
            estacionamiento.PorcentajeRecargo = request.PorcentajeRecargo;
            estacionamiento.DiasUmbralProporcional = request.DiasUmbralProporcional;

            _context.SaveChanges();
            return estacionamiento;
        }
    }
}

namespace Parkking_backend.Services
{
    public class ModificarEstacionamientoRequest
    {
        public int DiaVencimientoAbono { get; set; }
        public bool AplicaRecargo { get; set; }
        public decimal PorcentajeRecargo { get; set; }
        public int? DiasUmbralProporcional { get; set; }
    }
}
