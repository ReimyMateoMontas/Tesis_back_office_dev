namespace Api_Eden.Controllers;

using global::Api_Eden.DTOs.Zone.Request;
using global::Api_Eden.DTOs.Zone.Response;
using global::Api_Eden.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ZoneController : ControllerBase
{
    private readonly ZoneService _zoneService;

    public ZoneController(ZoneService zoneService) => _zoneService = zoneService;

    
    [HttpGet]

    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<ActionResult<IEnumerable<ZoneResponseDto>>> GetAll()
    {
        try { return Ok(await _zoneService.GetAllAsync()); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener zonas", error = ex.Message }); }
    }

    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ZoneResponseDto>> GetById(int id)
    {
        try { return Ok(await _zoneService.GetByIdAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener zona", error = ex.Message }); }
    }

    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    [HttpGet("{id}/animales")]
    public async Task<ActionResult> GetAnimalesByZona(int id)
    {
        try { return Ok(await _zoneService.GetAnimalesByZonaAsync(id)); }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener animales de la zona", error = ex.Message }); }
    }


    /// Devuelve el historial de movimientos de todas las zonas

    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    [HttpGet("movimientos")]
    public async Task<ActionResult> GetMovimientos()
    {
        try { return Ok(await _zoneService.GetMovimientosAsync()); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener movimientos", error = ex.Message }); }
    }

   
    /// Devuelve el historial de movimientos de un animal específico
    
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    [HttpGet("movimientos/{animalId}")]
    public async Task<ActionResult> GetMovimientosByAnimal(int animalId)
    {
        try { return Ok(await _zoneService.GetMovimientosByAnimalAsync(animalId)); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al obtener movimientos del animal", error = ex.Message }); }
    }


    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    [HttpPost("mover-animal")]
    public async Task<ActionResult> MoverAnimal([FromBody] MoverAnimalDto dto)
    {
        try
        {
   
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int usuarioId))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario" });

            await _zoneService.MoverAnimalAsync(dto, usuarioId);
            return Ok(new { mensaje = "Animal movido correctamente" });
        }
        catch (KeyNotFoundException ex) { return NotFound(new { mensaje = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al mover el animal", error = ex.Message }); }
    }

    [Authorize(Roles = "Administrador,Trabajador,Veterinario")]
    [HttpPost]
    public async Task<ActionResult<ZoneResponseDto>> Create([FromBody] CreateZoneDto dto)
    {
        try
        {
            var zone = await _zoneService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = zone.Id }, zone);
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al crear zona", error = ex.Message }); }
    }

    [Authorize(Roles = "Administrador,Trabajador,Veterinario")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateZoneDto dto)
    {
        try
        {
            var updated = await _zoneService.UpdateAsync(id, dto);
            if (!updated) return NotFound(new { mensaje = $"No existe una zona con ID {id}." });
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al actualizar zona", error = ex.Message }); }
    }

    [Authorize(Roles = "Administrador,Trabajador,Veterinario")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _zoneService.DeleteAsync(id);
            if (!deleted) return NotFound(new { mensaje = $"No existe una zona con ID {id}." });
            return NoContent();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
        catch (Exception ex) { return StatusCode(500, new { mensaje = "Error al eliminar zona", error = ex.Message }); }
    }
}