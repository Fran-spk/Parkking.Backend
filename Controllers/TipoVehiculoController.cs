using Microsoft.AspNetCore.Mvc;
using MODELO;
using Parkking_backend.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TipoVehiculoController : ControllerBase
    {
        private readonly TipoVehiculoService _service;

        public TipoVehiculoController(TipoVehiculoService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<TipoVehiculo>> GetTiposVehiculo(
            [FromQuery] bool includeInactivos = false)
        {
            try
            {
                var tipos = _service.GetAll(includeInactivos);
                return Ok(tipos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult AgregarTipoVehiculo([FromBody] string nombre)
        {
            try
            {
                var tipo = _service.Create(nombre);
                return Ok("Tipo de vehículo agregado exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult ModificarTipoVehiculo(int id, [FromBody] string nuevoNombre)
        {
            try
            {
                var tipo = _service.Update(id, nuevoNombre);
                return Ok("Tipo de vehículo modificado exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult DarDeBajaTipoVehiculo(int id)
        {
            try
            {
                _service.Deactivate(id);
                return Ok("Tipo de vehículo dado de baja exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/reactivar")]
        public ActionResult ReactivarTipoVehiculo(int id)
        {
            try
            {
                _service.Reactivate(id);
                return Ok("Tipo de vehículo reactivado exitosamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}