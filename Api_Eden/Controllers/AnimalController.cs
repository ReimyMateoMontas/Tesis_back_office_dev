namespace Api_Eden.Controllers;


using Api_Eden.DTOs.AnimalCreadoDto;
using Api_Eden.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;



[ApiController]
[Route("api/[controller]")]
public class AnimalController : ControllerBase
{
    private readonly AnimalService _animalService;

    public AnimalController(AnimalService animalService)
    {
        _animalService = animalService;
    }

    // GET: api/animal
    [HttpGet]
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<ActionResult<IEnumerable<AnimalDTO>>> GetAnimales()
    {
        try
        {
            var animales = await _animalService.GetAllAsync();
            return Ok(animales);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno del servidor", ex.Message });
        }
    }

    // GET: api/animal/{id}
    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<ActionResult<AnimalDTO>> GetAnimal(int id)
    {
        try
        {
            var animal = await _animalService.GetByIdAsync(id);
            if (animal == null) return NotFound(new { mensaje = "Animal no encontrado" });
            return Ok(animal);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error interno del servidor", ex.Message });
        }
    }

    // POST: api/animal
    [HttpPost]
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<ActionResult<AnimalDTO>> PostAnimal([FromBody] CrearAnimalDto dto)
    {
        try
        {
            var animal = await _animalService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetAnimal), new { id = animal.Id }, animal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al crear el animal", error = ex.Message });
        }
    }

    // PUT: api/animal/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<ActionResult> PutAnimal(int id, [FromBody] CrearAnimalDto dto)
    {
        try
        {
            var actualizado = await _animalService.UpdateAsync(id, dto);
            if (!actualizado) return NotFound(new { mensaje = "Animal no encontrado" });
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al actualizar el animal", error = ex.Message });
        }
    }

    // DELETE: api/animal/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DeleteAnimal(int id)
    {
        try
        {
            var eliminado = await _animalService.DeleteAsync(id);
            if (!eliminado) return NotFound(new { mensaje = "Animal no encontrado" });
            return Ok(new { mensaje = "Animal eliminado correctamente" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al eliminar el animal", error = ex.Message });
        }
    }


    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Administrador,Veterinario")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoAnimalDto dto)
    {
        try
        {
            var actualizado = await _animalService.ActualizarEstadoAsync(
                id, dto.EstadoGeneral, dto.EstadoSalud);

            if (!actualizado)
                return NotFound(new { mensaje = "Animal no encontrado." });

            return Ok(new { mensaje = "Estado actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al actualizar estado", error = ex.Message });
        }
    }

    // GET: api/animal/especie
    [HttpGet("especie")]
    [Authorize(Roles = "Administrador,Veterinario,Trabajador")]
    public async Task<IActionResult> GetEspecies()
    {
        try
        {
            var especies = await _animalService.GetEspeciesAsync();
            return Ok(especies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al obtener especies", error = ex.Message });
        }
    }
}

