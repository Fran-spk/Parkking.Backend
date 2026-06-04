using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Caja;
using Parkking_backend.Models;
using Parkking_backend.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CajaMensualController : ControllerBase
    {
        private readonly CajaMensualService _service;

        public CajaMensualController(CajaMensualService service)
        {
            _service = service;
        }

        // GET /api/cajamensual/mes?year=2026&month=3
        [HttpGet("mes")]
        public ActionResult GetByMes([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                var result = _service.GetByMes(year, month);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET /api/cajamensual/movimientos?year=2026&month=3&tipo=1
        [HttpGet("movimientos")]
        public ActionResult GetMovimientos(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] TipoMovimiento? tipo = null)
        {
            try
            {
                var result = _service.GetMovimientos(year, month, tipo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT /api/cajamensual/cerrar?year=2026&month=3
        [HttpPut("cerrar")]
        public ActionResult CerrarCaja([FromQuery] int year, [FromQuery] int month)
        {
            try
            {
                _service.Close(year, month);
                return Ok("Caja cerrada exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST /api/cajamensual/movimiento
        [HttpPost("movimiento")]
        public ActionResult RegistrarMovimiento([FromBody] RegistrarMovimientoRequest request)
        {
            try
            {
                var result = _service.RegisterMovimiento(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("movimientos/abono/{abonoCocheraId}")]
        public ActionResult GetMovimientosByAbono(int abonoCocheraId)
        {
            try
            {
                var result = _service.GetMovimientosByAbono(abonoCocheraId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
