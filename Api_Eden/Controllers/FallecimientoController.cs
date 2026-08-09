using Api_Eden.Data;
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
    public class FallecimientoController : ControllerBase
    {
        private readonly IFallecimientoService _service;

        public FallecimientoController(IFallecimientoService service)
        {
            _service = service;
        }

        // GET: api/fallecimiento
        // Veterinario también puede ver los fallecimientos
        [Authorize(Roles = "Administrador,Veterinario")]
        [HttpGet]
        public async Task<IActionResult> GetFallecimientos([FromServices] AppDbContext db)
        {
            try
            {
                var data = await db.Fallecimientos
                    .Include(f => f.Animal)
                    .Include(f => f.Veterinario)
                    .Include(f => f.UsuarioRegistro)
                    .OrderByDescending(f => f.Fecha)
                    .Select(f => new
                    {
                        f.Id,
                        Animal = f.Animal.Nombre,
                        AnimalId = f.AnimalId,
                        FotografiaUrl = f.Animal.FotografiaUrl,
                        f.Fecha,
                        f.Causa,
                        Veterinario = $"{f.Veterinario.Nombre} {f.Veterinario.Apellido}",
                        RegistradoPor = $"{f.UsuarioRegistro.Nombre} {f.UsuarioRegistro.Apellido}",
                        f.Observaciones,
                        f.FechaCreacion
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener fallecimientos", error = ex.Message });
            }
        }

       
        [Authorize(Roles = "Administrador,Veterinario")]
        [HttpPost]
        public async Task<IActionResult> RegistrarFallecimiento([FromBody] RegistrarFallecimientoDto dto)
        {
            try
            {
           
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out int userId))
                    return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

                var dtoCorregido = dto with
                {
                    VeterinarioId = userId,
                    UsuarioRegistroId = userId,
                };

                var (ok, mensaje) = await _service.RegistrarFallecimiento(dtoCorregido);

                if (!ok) return BadRequest(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar fallecimiento", error = ex.Message });
            }
        }
    }
}