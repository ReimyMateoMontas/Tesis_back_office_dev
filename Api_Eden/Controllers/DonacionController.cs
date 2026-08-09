


using Api_Eden.DTOs.DonacionesDto;
using Api_Eden.Services.DonacionesService.Interface;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonacionController : ControllerBase
    {
        private readonly IDonacionService _service;

        public DonacionController(IDonacionService service) => _service = service;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _service.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener donaciones", error = ex.Message }); }
        }

        [Authorize]
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            try { return Ok(await _service.GetResumenAsync()); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener resumen", error = ex.Message }); }
        }

        [Authorize]
        [HttpGet("tipos")]
        public async Task<IActionResult> GetTipos()
        {
            try { return Ok(await _service.GetTiposAsync()); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener tipos", error = ex.Message }); }
        }

        [Authorize]
        [HttpGet("donantes")]
        public async Task<IActionResult> GetDonantes()
        {
            try { return Ok(await _service.GetDonantesAsync()); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener donantes", error = ex.Message }); }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Registrar([FromBody] RegistrarDonacionDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            var (ok, mensaje, id) = await _service.RegistrarAsync(dto, userId);
            if (!ok) return BadRequest(new { mensaje });
            return Ok(new { mensaje, id });
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (ok, mensaje) = await _service.EliminarAsync(id);
            if (!ok) return NotFound(new { mensaje });
            return Ok(new { mensaje });
        }
    }
}