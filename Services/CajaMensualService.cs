using Microsoft.EntityFrameworkCore;
using MODELO;
using MODELO.Contexto;
using Parkking_backend.DTOs.Caja;
using Parkking_backend.Models;
using Parkking_backend.Models.Movimientos_Strategy;

namespace Parkking_backend.Services
{
    public class CajaMensualService
    {
        private readonly EstacionamientoContext _context;
        private readonly IEstacionamientoContext _estacionamiento;

        public CajaMensualService(
            EstacionamientoContext context,
            IEstacionamientoContext estacionamiento)
        {
            _context = context;
            _estacionamiento = estacionamiento;
        }

        public dynamic GetByMes(int year, int month)
        {
            var caja = _context.CajasMensuales
                .Include(c => c.Movimientos)
                .FirstOrDefault(c => c.Mes.Year == year
                                  && c.Mes.Month == month
                                  && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (caja == null)
                throw new Exception("No hay caja para ese mes");

            var result = new
            {
                caja.CajaMensualId,
                caja.Mes,
                caja.Cerrada,
                Movimientos = caja.Movimientos
                    .OrderByDescending(m => m.FechaHora)
                    .Select(m => MovimientoCajaMapper.ToDto(m))
            };

            return result;
        }

        public List<MovimientoCajaDto> GetMovimientos(int year, int month, TipoMovimiento? tipo = null)
        {
            var caja = _context.CajasMensuales
                .FirstOrDefault(c => c.Mes.Year == year
                                  && c.Mes.Month == month
                                  && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (caja == null)
                throw new Exception("No hay caja para ese mes");

            var query = _context.MovimientosCaja
                .Where(m => m.CajaMensualId == caja.CajaMensualId);

            if (tipo.HasValue)
                query = query.Where(m => m.Tipo == tipo.Value);

            var result = query
                .OrderByDescending(m => m.FechaHora)
                .ToList()
                .Select(m => MovimientoCajaMapper.ToDto(m))
                .ToList();

            return result;
        }

        public void Close(int year, int month)
        {
            var caja = _context.CajasMensuales
                .FirstOrDefault(c => c.Mes.Year == year
                                  && c.Mes.Month == month
                                  && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (caja == null)
                throw new Exception("No hay caja para ese mes");

            if (caja.Cerrada)
                throw new Exception("La caja ya está cerrada");

            caja.Cerrada = true;
            _context.SaveChanges();
        }

        public MovimientoCajaDto RegisterMovimiento(RegistrarMovimientoRequest request)
        {
            var caja = _context.CajasMensuales
                .FirstOrDefault(c => c.Mes.Year == request.Year
                                  && c.Mes.Month == request.Month
                                  && c.EstacionamientoId == _estacionamiento.EstacionamientoId);

            if (caja == null)
                throw new Exception("No hay caja abierta para ese mes");

            if (caja.Cerrada)
                throw new Exception("La caja del mes ya está cerrada");

            var strategy = MovimientoFactory.Crear(request, caja.CajaMensualId);
            var movimiento = strategy.CrearMovimiento();

            _context.MovimientosCaja.Add(movimiento);
            _context.SaveChanges();

            return MovimientoCajaMapper.ToDto(movimiento);
        }

        public List<MovimientoCajaDto> GetMovimientosByAbono(int abonoCocheraId)
        {
            var movimientos = _context.MovimientosCaja
                .Where(m => m.AbonoCocheraId == abonoCocheraId
                         && (m.TipoConcepto == TipoConcepto.CargoCliente
                          || m.TipoConcepto == TipoConcepto.ReintegroCliente))
                .OrderByDescending(m => m.FechaHora)
                .ToList()
                .Select(m => MovimientoCajaMapper.ToDto(m))
                .ToList();

            return movimientos;
        }
    }
}
