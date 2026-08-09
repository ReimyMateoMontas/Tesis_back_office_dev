using Api_Eden.DTOs.GastosDto;
using Api_Eden.Services.GastosService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api_Eden.Controllers
{
    

    [ApiController]
    [Route("api/[controller]")]
    public class GastoController : ControllerBase
    {
        private readonly IGastoService _gastoService;

        public GastoController(IGastoService gastoService) => _gastoService = gastoService;

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> GetGastos()
        {
            try
            {
                var gastos = await _gastoService.GetGastos();
                return Ok(gastos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener gastos", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGasto(int id)
        {
            try
            {
                var (ok, mensaje, data) = await _gastoService.GetGasto(id);
                if (!ok) return NotFound(new { mensaje });
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener gasto", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            try
            {
                var categorias = await _gastoService.GetCategorias();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener categorías", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> CrearGasto([FromBody] CrearGastoDto dto)
        {
            try
            {
                var (ok, mensaje) = await _gastoService.CrearGasto(dto);
                if (!ok) return BadRequest(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al crear gasto", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarGasto(int id, [FromBody] ActualizarGastoDto dto)
        {
            try
            {
                var (ok, mensaje) = await _gastoService.ActualizarGasto(id, dto);
                if (!ok) return BadRequest(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar gasto", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarGasto(int id)
        {
            try
            {
                var (ok, mensaje) = await _gastoService.EliminarGasto(id);
                if (!ok) return NotFound(new { mensaje });
                return Ok(new { mensaje });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar gasto", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("resumen/mensual")]
        public async Task<IActionResult> GetResumenMensual()
        {
            try
            {
                var resumen = await _gastoService.GetResumenMensual();
                return Ok(resumen);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener resumen", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("categoria/{year}/{mes}")]
        public async Task<IActionResult> GetGastosPorCategoria(int year, int mes)
        {
            try
            {
                var gastos = await _gastoService.GetGastosPorCategoria(year, mes);
                return Ok(gastos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener gastos por categoría", error = ex.Message });
            }
        }
        [Authorize(Roles = "Administrador")]
        [HttpGet("resumen/serie-mensual")]
        public async Task<IActionResult> GetSerieMensual([FromQuery] int? year)
        {
            try
            {
                var serie = await _gastoService.GetSerieMensual(year);
                return Ok(serie);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener la serie mensual", error = ex.Message });
            }
        }
    }
}
