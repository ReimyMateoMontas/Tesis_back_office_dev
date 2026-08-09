using Api_Eden.DTOs.AdopcionDto;
using Api_Eden.Services.AdopcionesService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdopcionController : ControllerBase
    {
        private readonly IAdopcionService _adopcionService;

        public AdopcionController(IAdopcionService adopcionService) => _adopcionService = adopcionService;

        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpGet]
        public async Task<IActionResult> GetAdopciones()
        {
            try
            {
                var adopciones = await _adopcionService.GetAdopciones();
                return Ok(adopciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener adopciones", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdopcion(int id)
        {
            try
            {
                var (ok, mensaje, data) = await _adopcionService.GetAdopcion(id);
                if (!ok) return NotFound(new { mensaje });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener adopción", error = ex.Message });
            }
        }

        
        [HttpPost]
        [Authorize(Roles = "Administrador,Trabajador")]
        public async Task<IActionResult> RegistrarAdopcion([FromBody] RegistrarAdopcionDto dto)
        {
            try
            {
                var (ok, mensaje) = await _adopcionService.RegistrarAdopcion(dto);
                if (!ok) return BadRequest(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar adopción", error = ex.Message });
            }
        }

       
        [HttpPut("{id}/estado")]
        [Authorize(Roles = "Administrador,Trabajador")]
        public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoAdopcionDto dto)
        {
            try
            {
                var (ok, mensaje) = await _adopcionService.ActualizarEstado(id, dto);
                if (!ok) return BadRequest(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar estado", error = ex.Message });
            }
        }
    }
}
