

using Api_Eden.DTOs.ObjectivoDto;

using Api_Eden.Services.ObjetivoService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObjetivoController : ControllerBase
    {
        private readonly IObjetivoService _service;

        public ObjetivoController(IObjetivoService service) => _service = service;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try { return Ok(await _service.GetAllAsync()); }
            catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener objetivos", error = ex.Message }); }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearObjetivoDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            var (ok, mensaje, id) = await _service.CrearAsync(dto, userId);
            if (!ok) return BadRequest(new { mensaje });
            return Ok(new { mensaje, id });
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarObjetivoDto dto)
        {
            var (ok, mensaje) = await _service.ActualizarAsync(id, dto);
            if (!ok) return NotFound(new { mensaje });
            return Ok(new { mensaje });
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