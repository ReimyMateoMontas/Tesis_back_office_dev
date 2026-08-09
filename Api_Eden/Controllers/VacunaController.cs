using Api_Eden.DTOs.MedicoDto;
using Api_Eden.Services.TratamientoService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacunaController : ControllerBase
    {
        private readonly IVacunaService _vacunaService;

        public VacunaController(IVacunaService vacunaService)
        {
            _vacunaService = vacunaService;
        }

        [Authorize]
        [HttpGet("animal/{animalId}")]
        public async Task<IActionResult> GetVacunasPorAnimal(int animalId)
        {
            var (ok, mensaje, data) = await _vacunaService.GetVacunasPorAnimal(animalId);

            if (!ok)
                return NotFound(new { mensaje });

            return Ok(data);
        }

        
        [Authorize(Roles = "Administrador,Veterinario")]
        [HttpPost]
        public async Task<IActionResult> RegistrarVacuna([FromBody] RegistrarVacunaDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            var dtoCorregido = dto with { VeterinarioId = userId };

            var (ok, mensaje, id) = await _vacunaService.RegistrarVacuna(dtoCorregido);

            if (!ok)
                return BadRequest(new { mensaje });

            return Ok(new { mensaje, id });
        }

        [Authorize]
        [HttpGet("todas")]
        public async Task<IActionResult> GetTodasLasVacunas([FromServices] Api_Eden.Data.AppDbContext db)
        {
            try
            {
                var data = await db.Vacunas
                    .Include(v => v.Animal)
                    .Include(v => v.TipoVacuna)
                    .Include(v => v.Veterinario)
                    .OrderByDescending(v => v.FechaAplicacion)
                    .Select(v => new
                    {
                        v.Id,
                        AnimalId = v.AnimalId,
                        Animal = v.Animal.Nombre,
                        FotografiaUrl = v.Animal.FotografiaUrl,
                        TipoVacuna = v.TipoVacuna.Nombre,
                        FechaAplicacion = v.FechaAplicacion.ToString("yyyy-MM-dd"),
                        ProximaDosis = v.ProximaDosis != null
                                          ? v.ProximaDosis.Value.ToString("yyyy-MM-dd") : null,
                        v.Lote,
                        Veterinario = v.Veterinario.Nombre + " " + v.Veterinario.Apellido,
                        v.Observaciones,
                        Vencida = v.ProximaDosis.HasValue &&
                                        v.ProximaDosis < DateOnly.FromDateTime(DateTime.Today),
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener vacunas", error = ex.Message });
            }
        }
    }
}