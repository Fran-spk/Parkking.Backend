using Microsoft.AspNetCore.Mvc;
using MODELO.seguridad;
using Parkking_backend.Services;

namespace Parkking_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GrupoController : ControllerBase
    {
        private readonly GrupoService _service;

        public GrupoController(GrupoService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Grupo>> GetAllGrupos()
        {
            try
            {
                var grupos = _service.GetAll();
                return Ok(grupos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("byestado/{estadoId}")]
        public ActionResult<IEnumerable<Grupo>> GetAllGruposByEstado(int estadoId)
        {
            try
            {
                var estados = _service.GetAllEstados();
                var estado = estados.FirstOrDefault(e => e.EST_GRU_ID == estadoId);
                if (estado == null)
                    return NotFound("Estado de grupo no encontrado");

                var grupos = _service.GetByEstado(estado);
                return Ok(grupos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("estados")]
        public ActionResult<IEnumerable<Estado_Grupo>> GetAllEstadosGrupo()
        {
            try
            {
                var estados = _service.GetAllEstados();
                return Ok(estados);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("modulos")]
        public ActionResult<IEnumerable<Modulo>> GetAllModulos()
        {
            try
            {
                var modulos = _service.GetAllModulos();
                return Ok(modulos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult AgregarGrupo([FromBody] Grupo grupo)
        {
            try
            {
                _service.Create(grupo);
                return Ok("Grupo agregado correctamente");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult EliminarGrupo(int id)
        {
            try
            {
                var grupo = _service.GetById(id);
                _service.Delete(grupo);
                return Ok("El grupo fue eliminado");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public ActionResult ModificarGrupo(int id, [FromBody] Grupo grupo)
        {
            try
            {
                grupo.GRU_ID = id;
                _service.Update(grupo);
                return Ok("El grupo fue modificado");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
