using Api_Eden.DTOs.InventarioAlimentos;
using Api_Eden.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Api_Eden.Data;


namespace Api_Eden.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class InventarioController : ControllerBase
    {

        private readonly AppDbContext _db;

        public InventarioController(AppDbContext db)
        {
            _db = db;

        }

        // GET: api/alimento
        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpGet]
        public async Task<IActionResult> GetAlimentos()
        {
            try
            {
                var alimentos = await _db.Alimentos
                    .Where(a => a.Activo == true)
                    .Select(a => new AlimentoDto(
                        a.Id,
                        a.Nombre,
                        a.TipoAnimal,
                        a.Marca,
                        a.UnidadMedida,
                        a.CantidadDisponible,
                        a.StockMinimo,
                        a.FechaVencimiento,
                        a.Activo,
                        a.CantidadDisponible <= a.StockMinimo  
                    ))
                    .ToListAsync();

                var stockBajo = alimentos.Where(a => a.StockBajo).ToList();

                return Ok(new
                {
                    total = alimentos.Count,
                    alertaStock = stockBajo.Count > 0
                        ? $"{stockBajo.Count} alimento(s) con stock bajo"
                        : "Stock en niveles normales",
                    alimentos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlimento(int id)
        {
            try
            {
                var a = await _db.Alimentos.FindAsync(id);

                if (a is null)
                    return NotFound(new { mensaje = "Alimento no encontrado." });

                var dto = new AlimentoDto(
                    a.Id,
                    a.Nombre,
                    a.TipoAnimal,
                    a.Marca,
                    a.UnidadMedida,
                    a.CantidadDisponible,
                    a.StockMinimo,
                    a.FechaVencimiento,
                    a.Activo,
                    a.CantidadDisponible <= a.StockMinimo
                );

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> PostAlimento([FromBody] CrearAlimentoDto dto)
        {
            try
            {
                var tiposValidos = new[] { "Perro", "Gato", "Ave", "Conejo", "Reptil", "Otro" };
                if (!tiposValidos.Contains(dto.TipoAnimal))
                    return BadRequest(new { mensaje = $"TipoAnimal inválido. Usa: {string.Join(", ", tiposValidos)}" });

                var alimento = new Alimento
                {
                    Nombre = dto.Nombre,
                    TipoAnimal = dto.TipoAnimal,
                    Marca = dto.Marca,
                    UnidadMedida = dto.UnidadMedida,
                    CantidadDisponible = dto.CantidadDisponible,
                    StockMinimo = dto.StockMinimo,
                    FechaVencimiento = dto.FechaVencimiento,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                _db.Alimentos.Add(alimento);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAlimento), new { id = alimento.Id },
                    new { mensaje = "Alimento registrado correctamente.", id = alimento.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar el alimento", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlimento(int id, [FromBody] ActualizarAlimentoDto dto)
        {
            try
            {
                var alimento = await _db.Alimentos.FindAsync(id);

                if (alimento is null)
                    return NotFound(new { mensaje = "Alimento no encontrado." });

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                    alimento.Nombre = dto.Nombre;

                if (!string.IsNullOrWhiteSpace(dto.TipoAnimal))
                {
                    var tiposValidos = new[] { "Perro", "Gato", "Ave", "Conejo", "Reptil", "Otro" };
                    if (!tiposValidos.Contains(dto.TipoAnimal))
                        return BadRequest(new { mensaje = $"TipoAnimal inválido. Usa: {string.Join(", ", tiposValidos)}" });
                    alimento.TipoAnimal = dto.TipoAnimal;
                }

                if (!string.IsNullOrWhiteSpace(dto.Marca))
                    alimento.Marca = dto.Marca;

                if (!string.IsNullOrWhiteSpace(dto.UnidadMedida))
                    alimento.UnidadMedida = dto.UnidadMedida;

                if (dto.CantidadDisponible.HasValue)
                    alimento.CantidadDisponible = dto.CantidadDisponible.Value;

                if (dto.StockMinimo.HasValue)
                    alimento.StockMinimo = dto.StockMinimo.Value;

                if (dto.FechaVencimiento.HasValue)
                    alimento.FechaVencimiento = dto.FechaVencimiento;

                if (dto.Activo.HasValue)
                    alimento.Activo = dto.Activo;

                await _db.SaveChangesAsync();

                return Ok(new { mensaje = "Alimento actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al actualizar el alimento", error = ex.Message });
            }
        }

        // DELETE: api/alimento/{id}
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlimento(int id)
        {
            try
            {
                var alimento = await _db.Alimentos.FindAsync(id);

                if (alimento is null)
                    return NotFound(new { mensaje = "Alimento no encontrado." });

                // Baja lógica — no elimina el registro, solo lo desactiva
                alimento.Activo = false;
                await _db.SaveChangesAsync();

                return Ok(new { mensaje = "Alimento desactivado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al eliminar el alimento", error = ex.Message });
            }
        }
    

        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpPost("{id}/entrada")]
        public async Task<IActionResult> RegistrarEntrada(int id, [FromBody] RegistrarMovimientoDto dto)
        {
            try
            {
                var alimento = await _db.Alimentos.FindAsync(id);
                if (alimento is null)
                    return NotFound(new { mensaje = "Alimento no encontrado." });

                alimento.CantidadDisponible += dto.Cantidad;

                var movimiento = new Movimientosinventario
                {
                    AlimentoId = id,
                    TipoMovimiento = "Entrada",
                    Cantidad = dto.Cantidad,
                    Motivo = dto.Motivo ?? "Reposición de stock",
                    FechaMovimiento = DateTime.UtcNow,
                    UsuarioResponsableId = dto.UsuarioResponsableId,
                    Observaciones = dto.Observaciones,
                    CostoUnitario = dto.CostoUnitario,
                };

                _db.Movimientosinventarios.Add(movimiento);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Entrada registrada correctamente.",
                    nuevoStock = alimento.CantidadDisponible,
                    stockBajo = alimento.CantidadDisponible <= alimento.StockMinimo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar entrada", error = ex.Message });
            }
        }

        // POST: api/inventario/{id}/salida
        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpPost("{id}/salida")]
        public async Task<IActionResult> RegistrarSalida(int id, [FromBody] RegistrarMovimientoDto dto)
        {
            try
            {
                var alimento = await _db.Alimentos.FindAsync(id);
                if (alimento is null)
                    return NotFound(new { mensaje = "Alimento no encontrado." });

                if (dto.Cantidad > alimento.CantidadDisponible)
                    return BadRequest(new
                    {
                        mensaje = $"Stock insuficiente. Disponible: {alimento.CantidadDisponible} {alimento.UnidadMedida}"
                    });

                alimento.CantidadDisponible -= dto.Cantidad;

                var movimiento = new Movimientosinventario
                {
                    AlimentoId = id,
                    TipoMovimiento = "Salida",
                    Cantidad = dto.Cantidad,
                    Motivo = dto.Motivo ?? "Consumo",
                    FechaMovimiento = DateTime.UtcNow,
                    UsuarioResponsableId = dto.UsuarioResponsableId,
                    Observaciones = dto.Observaciones,
                };

                _db.Movimientosinventarios.Add(movimiento);
                await _db.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Salida registrada correctamente.",
                    nuevoStock = alimento.CantidadDisponible,
                    stockBajo = alimento.CantidadDisponible <= alimento.StockMinimo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar salida", error = ex.Message });
            }
        }

        // GET: api/inventario/{id}/movimientos
        [Authorize(Roles = "Administrador,Trabajador")]
        [HttpGet("{id}/movimientos")]
        public async Task<IActionResult> GetMovimientos(int id)
        {
            try
            {
                var movimientos = await _db.Movimientosinventarios
                    .Include(m => m.UsuarioResponsable)
                    .Where(m => m.AlimentoId == id)
                    .OrderByDescending(m => m.FechaMovimiento)
                    .Select(m => new
                    {
                        m.Id,
                        m.AlimentoId,
                        m.TipoMovimiento,
                        m.Cantidad,
                        m.Motivo,
                        FechaMovimiento = m.FechaMovimiento ?? DateTime.UtcNow,
                        UsuarioResponsable = $"{m.UsuarioResponsable.Nombre} {m.UsuarioResponsable.Apellido}",
                        m.Observaciones,
                        m.CostoUnitario,
                    })
                    .ToListAsync();

                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener movimientos", error = ex.Message });
            }
        }
    }
}
