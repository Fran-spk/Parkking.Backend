using Microsoft.AspNetCore.Mvc;
using Parkking_backend.DTOs.Dashboard;
using Parkking_backend.Services;

namespace Parkking_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public ActionResult<DashboardAbonosDto> GetSummary()
        {
            try
            {
                var dashboard = _service.GetSummary();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}