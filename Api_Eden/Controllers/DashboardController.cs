
using Api_Eden.Services.Dashboard.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetResumen()
        {
            try
            {
                var resumen = await _dashboardService.GetResumenAsync();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener resumen del dashboard", error = ex.Message });
            }
        }
    }
}