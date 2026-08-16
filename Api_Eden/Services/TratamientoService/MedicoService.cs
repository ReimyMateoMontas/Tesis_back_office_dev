using Api_Eden.Data;
using Api_Eden.DTOs.MedicoDto;
using Api_Eden.Models;
using Api_Eden.Services.TratamientoService.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.TratamientoService
{
    public class MedicoService : IMedicoService
    {
        private readonly AppDbContext _db;

        public MedicoService(AppDbContext db) => _db = db;

        // ── Historial médico 
        public async Task<IEnumerable<object>> GetHistorialAsync(int animalId)
        {
            return await _db.Historialmedicos
                .Where(h => h.AnimalId == animalId)
                .Include(h => h.Tratamientos)
                    .ThenInclude(t => t.Medicamento)
                .Include(h => h.Veterinario)
                .OrderByDescending(h => h.Fecha)
                .Select(h => (object)new
                {
                    h.Id,
                    h.AnimalId,
                    h.Fecha,
                    h.Diagnostico,
                    h.Sintomas,
                    h.Peso,
                    h.Temperatura,
                    h.Observaciones,
                    Veterinario = h.Veterinario.Nombre + " " + h.Veterinario.Apellido,
                    Tratamientos = h.Tratamientos.Select(t => new
                    {
                        t.Id,
                        Medicamento = t.Medicamento.Nombre,
                        t.Dosis,
                        t.Frecuencia,
                        t.ViaAdministracion,
                        t.Estado,
                        FechaInicio = t.FechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = t.FechaFin != null ? t.FechaFin.Value.ToString("yyyy-MM-dd") : null
                    })
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetTimelineAnimalAsync(int animalId)
        {
            var timeline = new List<(DateTime Fecha, object Item)>();

            var consultas = await _db.Historialmedicos
                .Where(h => h.AnimalId == animalId)
                .Include(h => h.Tratamientos).ThenInclude(t => t.Medicamento)
                .Include(h => h.Veterinario)
                .ToListAsync();

            foreach (var h in consultas)
            {
                timeline.Add((h.Fecha ?? h.FechaCreacion ?? DateTime.MinValue, new
                {
                    Tipo = "consulta",
                    h.Id,
                    Fecha = (h.Fecha ?? h.FechaCreacion)?.ToString("yyyy-MM-dd"),
                    Titulo = h.Diagnostico,
                    h.Sintomas,
                    h.Peso,
                    h.Temperatura,
                    h.Observaciones,
                    Veterinario = h.Veterinario.Nombre + " " + h.Veterinario.Apellido,
                    Tratamientos = h.Tratamientos.Select(t => new
                    {
                        t.Id,
                        Medicamento = t.Medicamento.Nombre,
                        t.Dosis,
                        t.Frecuencia,
                        t.ViaAdministracion,
                        t.Estado,
                        FechaInicio = t.FechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = t.FechaFin != null ? t.FechaFin.Value.ToString("yyyy-MM-dd") : null
                    }).ToList()
                }));
            }

            // ─ Vacunas ─
            var vacunas = await _db.Vacunas
                .Where(v => v.AnimalId == animalId)
                .Include(v => v.TipoVacuna)
                .Include(v => v.Veterinario)
                .ToListAsync();

            foreach (var v in vacunas)
            {
                var fechaVacuna = v.FechaAplicacion.ToDateTime(TimeOnly.MinValue);
                timeline.Add((fechaVacuna, new
                {
                    Tipo = "vacuna",
                    v.Id,
                    Fecha = v.FechaAplicacion.ToString("yyyy-MM-dd"),
                    Titulo = v.TipoVacuna.Nombre,
                    ProximaDosis = v.ProximaDosis?.ToString("yyyy-MM-dd"),
                    v.Lote,
                    v.Observaciones,
                    Veterinario = v.Veterinario.Nombre + " " + v.Veterinario.Apellido,
                    Vencida = v.ProximaDosis.HasValue &&
                              v.ProximaDosis < DateOnly.FromDateTime(DateTime.Today)
                }));
            }

            // ─ Fallecimiento ─
            var fallecimiento = await _db.Fallecimientos
                .Where(f => f.AnimalId == animalId)
                .Include(f => f.Veterinario)
                .Include(f => f.UsuarioRegistro)
                .FirstOrDefaultAsync();

            if (fallecimiento != null)
            {
                var fechaF = fallecimiento.Fecha.ToDateTime(TimeOnly.MinValue);
                timeline.Add((fechaF, new
                {
                    Tipo = "fallecimiento",
                    fallecimiento.Id,
                    Fecha = fallecimiento.Fecha.ToString("yyyy-MM-dd"),
                    Titulo = fallecimiento.Causa,
                    fallecimiento.Lugar,
                    fallecimiento.Observaciones,
                    Veterinario = fallecimiento.Veterinario != null
                        ? fallecimiento.Veterinario.Nombre + " " + fallecimiento.Veterinario.Apellido
                        : null,
                    RegistradoPor = fallecimiento.UsuarioRegistro.Nombre + " " + fallecimiento.UsuarioRegistro.Apellido
                }));
            }

            return timeline
                .OrderByDescending(x => x.Fecha)
                .Select(x => x.Item)
                .ToList();
        }

        // ── Historial completo estructurado por secciones ─────────────────────
        public async Task<object?> GetHistorialCompletoAnimalAsync(int animalId)
        {
            var animal = await _db.Animales
                .Include(a => a.Especie)
                .FirstOrDefaultAsync(a => a.Id == animalId);

            if (animal is null) return null;

            var consultas = await _db.Historialmedicos
                .Where(h => h.AnimalId == animalId)
                .Include(h => h.Tratamientos).ThenInclude(t => t.Medicamento)
                .Include(h => h.Veterinario)
                .OrderByDescending(h => h.Fecha)
                .Select(h => new
                {
                    Tipo = "consulta",
                    h.Id,
                    Fecha = h.Fecha,
                    h.Diagnostico,
                    h.Sintomas,
                    h.Peso,
                    h.Temperatura,
                    h.Observaciones,
                    Veterinario = h.Veterinario.Nombre + " " + h.Veterinario.Apellido,
                    Tratamientos = h.Tratamientos.Select(t => new
                    {
                        t.Id,
                        Medicamento = t.Medicamento.Nombre,
                        t.Dosis,
                        t.Frecuencia,
                        t.ViaAdministracion,
                        t.Estado,
                        FechaInicio = t.FechaInicio.ToString("yyyy-MM-dd"),
                        FechaFin = t.FechaFin != null ? t.FechaFin.Value.ToString("yyyy-MM-dd") : null
                    })
                })
                .ToListAsync<object>();

            var vacunas = await _db.Vacunas
                .Where(v => v.AnimalId == animalId)
                .Include(v => v.TipoVacuna)
                .Include(v => v.Veterinario)
                .OrderByDescending(v => v.FechaAplicacion)
                .Select(v => new
                {
                    Tipo = "vacuna",
                    v.Id,
                    Fecha = v.FechaAplicacion.ToString("yyyy-MM-dd"),
                    TipoVacuna = v.TipoVacuna.Nombre,
                    ProximaDosis = v.ProximaDosis != null ? v.ProximaDosis.Value.ToString("yyyy-MM-dd") : null,
                    v.Lote,
                    Veterinario = v.Veterinario.Nombre + " " + v.Veterinario.Apellido,
                    v.Observaciones,
                    Vencida = v.ProximaDosis.HasValue &&
                              v.ProximaDosis < DateOnly.FromDateTime(DateTime.Today)
                })
                .ToListAsync<object>();

            var fallecimiento = await _db.Fallecimientos
                .Where(f => f.AnimalId == animalId)
                .Include(f => f.Veterinario)
                .Include(f => f.UsuarioRegistro)
                .Select(f => (object?)new
                {
                    Tipo = "fallecimiento",
                    f.Id,
                    Fecha = f.Fecha.ToString("yyyy-MM-dd"),
                    f.Causa,
                    f.Lugar,
                    Veterinario = f.Veterinario != null
                        ? f.Veterinario.Nombre + " " + f.Veterinario.Apellido
                        : null,
                    RegistradoPor = f.UsuarioRegistro.Nombre + " " + f.UsuarioRegistro.Apellido,
                    f.Observaciones
                })
                .FirstOrDefaultAsync();

            return new
            {
                Animal = new
                {
                    animal.Id,
                    animal.Nombre,
                    Especie = animal.Especie?.Nombre,
                    animal.Raza,
                    FotografiaUrl = animal.FotografiaUrl,
                    animal.EstadoSalud,
                    animal.EstadoGeneral
                },
                Consultas = consultas,
                Vacunas = vacunas,
                Fallecimiento = fallecimiento
            };
        }

        public async Task<(bool ok, string mensaje, int? id)> RegistrarHistorialAsync(
            RegistrarHistorialDto dto)
        {
            var animal = await _db.Animales.FindAsync(dto.AnimalId);
            if (animal is null)
                return (false, "Animal no encontrado.", null);

            var responsable = await _db.Usuarios.FindAsync(dto.VeterinarioId);
            if (responsable is null ||
                (responsable.Rol != "Veterinario" && responsable.Rol != "Administrador"))
                return (false, "El usuario responsable no existe o no tiene permisos.", null);

            var historial = new Historialmedico
            {
                AnimalId = dto.AnimalId,
                Diagnostico = dto.Diagnostico,
                Sintomas = dto.Sintomas,
                Peso = dto.Peso,
                Temperatura = dto.Temperatura,
                VeterinarioId = dto.VeterinarioId,
                Observaciones = dto.Observaciones,
                Fecha = DateTime.UtcNow,
                FechaCreacion = DateTime.UtcNow,
            };

            _db.Historialmedicos.Add(historial);
            await _db.SaveChangesAsync();
            return (true, "Historial registrado correctamente.", historial.Id);
        }


        public async Task<IEnumerable<object>> GetTratamientosAsync()
        {
            return await _db.Tratamientos
                .Include(t => t.Medicamento)
                .Include(t => t.HistorialMedico)
                    .ThenInclude(h => h.Animal)
                        .ThenInclude(a => a.Especie)
                .Include(t => t.Veterinario)
                .OrderByDescending(t => t.FechaInicio)
                .Select(t => (object)new
                {
                    t.Id,
                    AnimalId = t.HistorialMedico.AnimalId,
                    Animal = t.HistorialMedico.Animal.Nombre,
                    FotografiaUrl = t.HistorialMedico.Animal.FotografiaUrl,
                    Especie = t.HistorialMedico.Animal.Especie != null
                                ? t.HistorialMedico.Animal.Especie.Nombre : null,
                    Diagnostico = t.HistorialMedico.Diagnostico,
                    Medicamento = t.Medicamento.Nombre,
                    t.Dosis,
                    t.Frecuencia,
                    t.ViaAdministracion,
                    t.Estado,
                    Veterinario = t.Veterinario.Nombre + " " + t.Veterinario.Apellido,
                    FechaInicio = t.FechaInicio.ToString("yyyy-MM-dd"),
                    FechaFin = t.FechaFin != null ? t.FechaFin.Value.ToString("yyyy-MM-dd") : null,
                    HistorialMedicoId = t.HistorialMedicoId,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetAllTratamientosAsync() =>
            await GetTratamientosAsync();

        // ── Medicamentos ──────────────────────────────────────────────────────
        public async Task<IEnumerable<object>> GetMedicamentosAsync()
        {
            return await _db.Medicamentos
                .Where(m => m.Activo == true)
                .OrderBy(m => m.Nombre)
                .Select(m => (object)new
                {
                    m.Id,
                    m.Nombre,
                    m.PrincipioActivo,
                    m.Presentacion,
                    m.Concentracion
                })
                .ToListAsync();
        }

        public async Task<(bool ok, object? data)> CrearMedicamentoAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, null);

            var existente = await _db.Medicamentos
                .FirstOrDefaultAsync(m => m.Nombre.ToLower() == nombre.ToLower().Trim());

            if (existente != null)
                return (true, new { id = existente.Id, nombre = existente.Nombre });

            var nuevo = new Medicamento
            {
                Nombre = nombre.Trim(),
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
            };

            _db.Medicamentos.Add(nuevo);
            await _db.SaveChangesAsync();
            return (true, new { id = nuevo.Id, nombre = nuevo.Nombre });
        }

        public async Task<IEnumerable<object>> GetTiposVacunaAsync()
        {
            return await _db.Tiposvacunas
                .Where(t => t.Activa == true)
                .OrderBy(t => t.Nombre)
                .Select(t => (object)new
                {
                    t.Id,
                    t.Nombre,
                    t.EspecieId,
                    t.Descripcion,
                    t.DuracionMeses,
                    t.Obligatoria
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<object>> GetVeterinariosAsync()
        {
            return await _db.Usuarios
                .Where(u => (u.Rol == "Veterinario" || u.Rol == "Administrador") && u.Activo == true)
                .OrderBy(u => u.Nombre)
                .Select(u => new { u.Id, u.Nombre, u.Apellido, u.Rol })
                .ToListAsync();
        }
    }
}