using Api_Eden.Data;
using Api_Eden.DTOs.MedicoDto;
using Api_Eden.Models;
using Api_Eden.Services.TratamientoService.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.TratamientoService
{
    public class FallecimientoService : IFallecimientoService
    {
        private readonly AppDbContext _db;

        public FallecimientoService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool ok, string mensaje)> RegistrarFallecimiento(RegistrarFallecimientoDto dto)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
               
                var animal = await _db.Animales.FindAsync(dto.AnimalId);
                if (animal is null)
                    return (false, "Animal no encontrado.");

                if (animal.EstadoGeneral == "Fallecido")
                    return (false, "El animal ya está registrado como fallecido.");

                
                var yaExiste = await _db.Fallecimientos
                    .AnyAsync(f => f.AnimalId == dto.AnimalId);

                if (yaExiste)
                    return (false, "Este animal ya tiene un registro de fallecimiento.");

                
                var veterinario = await _db.Usuarios.FindAsync(dto.VeterinarioId);
                if (veterinario is null || (veterinario.Rol != "Veterinario" && veterinario.Rol != "Administrador"))
                    return (false, "El usuario no existe o no tiene permisos para registrar vacunas.");

                var usuario = await _db.Usuarios.FindAsync(dto.UsuarioRegistroId);
                if (usuario is null)
                    return (false, "El usuario que registra no existe.");

                // VALIDAR FECHA
                var hoy = DateOnly.FromDateTime(DateTime.Today);

                if (dto.FechaFallecimiento > hoy)
                    return (false, "La fecha de fallecimiento no puede ser futura.");

               
                if (string.IsNullOrWhiteSpace(dto.CausaFallecimiento))
                    return (false, "La causa de fallecimiento es obligatoria.");

               
                var fallecimiento = new Fallecimiento
                {
                    AnimalId = dto.AnimalId,
                    Fecha = dto.FechaFallecimiento,
                    Causa = dto.CausaFallecimiento.Trim(),
                    VeterinarioId = dto.VeterinarioId,
                    UsuarioRegistroId = dto.UsuarioRegistroId,
                    Observaciones = dto.Observaciones?.Trim(),
                    FechaCreacion = DateTime.UtcNow
                };

                _db.Fallecimientos.Add(fallecimiento);

                // LÓGICA DE NEGOCIO
                animal.EstadoGeneral = "Fallecido";
                animal.EstadoSalud = null;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Fallecimiento registrado correctamente.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error interno: {ex.Message}");
            }
        }
    }
    }
