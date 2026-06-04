using Microsoft.AspNetCore.Mvc;
using Parkking_backend.Models;
using Parkking_backend.Services;

namespace Parkking_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EstacionamientoController : ControllerBase
    {
        private readonly EstacionamientoService _service;

        public EstacionamientoController(EstacionamientoService service)
        {
            _service = service;
        }

        // GET /api/estacionamiento
        [HttpGet]
        public ActionResult<Estacionamiento> Get()
        {
            try
            {
                var estacionamiento = _service.Get();
                return Ok(estacionamiento);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // PUT /api/estacionamiento
        [HttpPut]
        public ActionResult Modificar([FromBody] ModificarEstacionamientoRequest request)
        {
            try
            {
                var estacionamiento = _service.Update(request);
                return Ok("Configuración actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
