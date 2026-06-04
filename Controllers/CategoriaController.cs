namespace Parkking_backend.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Parkking_backend.Models;
    using Parkking_backend.Services;

    namespace API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class CategoriaCocheraController : ControllerBase
        {
            private readonly CategoriaService _service;

            public CategoriaCocheraController(CategoriaService service)
            {
                _service = service;
            }

            // GET /api/categoriacochera
            [HttpGet]
            public ActionResult<IEnumerable<CategoriaCochera>> GetAll(
                [FromQuery] bool includeInactivos = false)
            {
                try
                {
                    var categorias = _service.GetAll(includeInactivos);
                    return Ok(categorias);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            // POST /api/categoriacochera
            [HttpPost]
            public ActionResult AgregarCategoria([FromBody] CategoriaCocheraRequest request)
            {
                try
                {
                    var categoria = _service.Create(request);
                    return Ok(categoria);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            // PUT /api/categoriacochera/{id}
            [HttpPut("{id}")]
            public ActionResult ModificarCategoria(int id, [FromBody] CategoriaCocheraRequest request)
            {
                try
                {
                    var categoria = _service.Update(id, request);
                    return Ok(categoria);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            // DELETE /api/categoriacochera/{id}
            [HttpDelete("{id}")]
            public ActionResult DarDeBaja(int id)
            {
                try
                {
                    _service.Deactivate(id);
                    return Ok("Categoría dada de baja exitosamente");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            // PUT /api/categoriacochera/{id}/reactivar
            [HttpPut("{id}/reactivar")]
            public ActionResult Reactivar(int id)
            {
                try
                {
                    _service.Reactivate(id);
                    return Ok("Categoría reactivada exitosamente");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            public class CategoriaCocheraRequest
            {
                public string Nombre { get; set; }
                public bool Activo { get; set; } = true;
            }
        }
    }
}
