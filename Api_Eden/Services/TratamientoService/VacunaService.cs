using Api_Eden.Data;
using Api_Eden.DTOs.MedicoDto;
using Api_Eden.Models;
using Api_Eden.Services.TratamientoService.Interface;
using Microsoft.EntityFrameworkCore;


namespace Api_Eden.Services.TratamientoService
{


    public class VacunaService : IVacunaService
    {
        private readonly AppDbContext _db;
        public VacunaService(AppDbContext db)
        {
            _db = db;
        }
        public async Task<(bool ok, string mensaje, int? id)> RegistrarVacuna(RegistrarVacunaDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                //  VALIDAR ANIMAL
                var animal = await _db.Animales.FindAsync(dto.AnimalId);
                if (animal is null)
                    return (false, "Animal no encontrado.", null);
               

                // VALIDAR QUE EL ANIMAL NO ESTÉ FALLECIDO
                if (animal.FechaFallecimiento.HasValue)
                    return (false, "No se puede registrar una vacuna. El animal está fallecido.", null);

                // VALIDAR TIPO DE VACUNA
                var tipoVacuna = await _db.Tiposvacunas.FindAsync(dto.TipoVacunaId);
                if (tipoVacuna is null)
                    return (false, "Tipo de vacuna no encontrado.", null);

                //VALIDAR VETERINARIO
                var veterinario = await _db.Usuarios.FindAsync(dto.VeterinarioId);
                if (veterinario is null || (veterinario.Rol != "Veterinario" && veterinario.Rol != "Administrador"))
                return (false, "El usuario no existe o no tiene permisos para registrar vacunas.", null);

                    //  VALIDAR FECHAS
                    var hoy = DateOnly.FromDateTime(DateTime.Today);

                if (dto.FechaAplicacion > hoy)
                    return (false, "La fecha de aplicación no puede ser futura.", null);

                if (dto.ProximaDosis.HasValue && dto.ProximaDosis < dto.FechaAplicacion)
                    return (false, "La próxima dosis no puede ser menor que la fecha de aplicación.", null);

                // VALIDAR DUPLICADOS (MISMA VACUNA MISMO DÍA)
                var yaExiste = await _db.Vacunas.AnyAsync(v =>
                    v.AnimalId == dto.AnimalId &&
                    v.TipoVacunaId == dto.TipoVacunaId &&
                    v.FechaAplicacion == dto.FechaAplicacion);

                if (yaExiste)
                    return (false, "Esta vacuna ya fue registrada para este animal en esa fecha.", null);

                // VALIDAR LOTE (OPCIONAL PERO LIMPIO)
                var lote = dto.Lote?.Trim();
                if (!string.IsNullOrEmpty(lote) && lote.Length > 50)
                    return (false, "El lote no puede exceder 50 caracteres.", null);

                // CREAR VACUNA
                var vacuna = new Vacuna
                {
                    AnimalId = dto.AnimalId,
                    TipoVacunaId = dto.TipoVacunaId,
                    FechaAplicacion = dto.FechaAplicacion,
                    ProximaDosis = dto.ProximaDosis,
                    Lote = lote,
                    VeterinarioId = dto.VeterinarioId,
                    Observaciones = dto.Observaciones?.Trim(),
                    FechaCreacion = DateTime.UtcNow
                };
                if (!dto.ProximaDosis.HasValue)
                {
                    vacuna.ProximaDosis = dto.FechaAplicacion.AddMonths(12);
                }
                _db.Vacunas.Add(vacuna);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Vacuna registrada correctamente.", vacuna.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                var inner = ex.InnerException?.Message ?? "Sin inner exception";
                return (false, $"Error interno: {ex.Message} | Inner: {inner}", null);
            }
           
        }
    
    public async Task<(bool ok, string mensaje, object? data)> GetVacunasPorAnimal(int animalId)
        {
            try
            {
                var animal = await _db.Animales.FindAsync(animalId);
                if (animal is null)
                    return (false, "Animal no encontrado.", null);

                var vacunas = await _db.Vacunas
                    .Where(v => v.AnimalId == animalId)
                    .Include(v => v.TipoVacuna)
                    .Include(v => v.Veterinario)
                    .OrderByDescending(v => v.FechaAplicacion)
                    .Select(v => new
                    {
                        v.Id,
                        TipoVacuna = v.TipoVacuna.Nombre,
                        v.FechaAplicacion,
                        v.ProximaDosis,
                        v.Lote,
                        Veterinario = $"{v.Veterinario.Nombre} {v.Veterinario.Apellido}",
                        v.Observaciones,
                        Vencida = v.ProximaDosis.HasValue &&
                                  v.ProximaDosis < DateOnly.FromDateTime(DateTime.Today)
                    })
                    .ToListAsync();

                if (!vacunas.Any())
                    return (false, "El animal no tiene vacunas registradas.", null);

                return (true, "OK", vacunas);
            }
            catch (Exception ex)
            {
                return (false, $"Error al obtener vacunas: {ex.Message}", null);
            }
        } }
    }
