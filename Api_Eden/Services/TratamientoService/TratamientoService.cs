using Api_Eden.Data;
using Api_Eden.DTOs.MedicoDto;
using Api_Eden.Models;
using Api_Eden.Services.TratamientoService.Interface;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Api_Eden.Services.TratamientoService
{
    public class TratamientoService : ITratamientoService
    {
        private readonly AppDbContext _db;

        public TratamientoService(AppDbContext db) => _db = db;

      
        public async Task<(bool ok, string mensaje, int? id)> RegistrarTratamiento(RegistrarTratamientoDto dto)
        {
            try
            {
                // ── Validaciones (lecturas: no disparan el bug) ───────────────────
                var historial = await _db.Historialmedicos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.Id == dto.HistorialMedicoId);
                if (historial is null)
                    return (false, "Historial médico no encontrado.", null);

                var medicamentoExiste = await _db.Medicamentos
                    .AnyAsync(m => m.Id == dto.MedicamentoId);
                if (!medicamentoExiste)
                    return (false, "Medicamento no encontrado.", null);

                var animalExiste = await _db.Animales
                    .AnyAsync(a => a.Id == historial.AnimalId);
                if (!animalExiste)
                    return (false, "Animal no encontrado.", null);

                if (dto.FechaFin < dto.FechaInicio)
                    return (false, "La fecha fin no puede ser menor que la fecha inicio.", null);

                // ── Datos normalizados para el INSERT ─────────────────────────────
                var fechaInicio = DateOnly.FromDateTime(dto.FechaInicio).ToString("yyyy-MM-dd");
                var fechaFin = DateOnly.FromDateTime(dto.FechaFin).ToString("yyyy-MM-dd");
                var fechaCreacion = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                int? nuevoId = null;

               
                await _db.Database.OpenConnectionAsync();
                try
                {
                    await _db.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO tratamientos
                            (historial_medico_id, medicamento_id, dosis, frecuencia,
                             via_administracion, fecha_inicio, fecha_fin, veterinario_id,
                             observaciones, estado, fecha_creacion)
                          VALUES (@histId, @medId, @dosis, @frec, @via, @fIni, @fFin, @vetId, @obs, 'Activo', @fCrea)",
                        new MySqlParameter("@histId", dto.HistorialMedicoId),
                        new MySqlParameter("@medId", dto.MedicamentoId),
                        new MySqlParameter("@dosis", dto.Dosis),
                        new MySqlParameter("@frec", dto.Frecuencia),
                        new MySqlParameter("@via", dto.ViaAdministracion),
                        new MySqlParameter("@fIni", fechaInicio),
                        new MySqlParameter("@fFin", fechaFin),
                        new MySqlParameter("@vetId", dto.VeterinarioId),
                        new MySqlParameter("@obs", MySqlDbType.Text)
                        {
                            Value = string.IsNullOrWhiteSpace(dto.Observaciones)
                                ? DBNull.Value
                                : dto.Observaciones
                        },
                        new MySqlParameter("@fCrea", fechaCreacion)
                    );

                    var ids = await _db.Database
                        .SqlQueryRaw<long>("SELECT LAST_INSERT_ID() AS `Value`")
                        .ToListAsync();
                    if (ids.Count > 0) nuevoId = (int)ids[0];
                }
                finally
                {
                    await _db.Database.CloseConnectionAsync();
                }

                await ActualizarEstadoSaludAnimalSeguro(historial.AnimalId, "EnTratamiento");

                return (true, "Tratamiento registrado correctamente.", nuevoId);
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? string.Empty;
                return (false, $"Error al registrar tratamiento: {ex.Message}{(string.IsNullOrEmpty(inner) ? "" : " | " + inner)}", null);
            }
        }

        public async Task<(bool ok, string mensaje)> ActualizarEstadoTratamiento(
            int id, string estado, int veterinarioId)
        {
            var estadosValidos = new[] { "Activo", "Completado", "Suspendido" };
            if (!estadosValidos.Contains(estado))
                return (false, "Estado inválido. Usa: Activo, Completado o Suspendido.");

            try
            {
                var tratamiento = await _db.Tratamientos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == id);
                if (tratamiento is null)
                    return (false, "Tratamiento no encontrado.");

               
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE tratamientos SET estado = {0} WHERE id = {1}",
                    estado, id);

                if (estado == "Completado")
                {
                    var historial = await _db.Historialmedicos
                        .AsNoTracking()
                        .FirstOrDefaultAsync(h => h.Id == tratamiento.HistorialMedicoId);

                    if (historial is not null)
                    {
                       
                        var tieneActivos = await _db.Tratamientos
                            .AnyAsync(t => t.HistorialMedico.AnimalId == historial.AnimalId
                                        && t.Id != id
                                        && t.Estado == "Activo");

                        if (!tieneActivos)
                            await ActualizarEstadoSaludAnimalSeguro(historial.AnimalId, "Recuperado");
                    }
                }

                return (true, $"Tratamiento actualizado a {estado}.");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? string.Empty;
                return (false, $"Error al actualizar tratamiento: {ex.Message}{(string.IsNullOrEmpty(inner) ? "" : " | " + inner)}");
            }
        }

        private async Task ActualizarEstadoSaludAnimalSeguro(int animalId, string estadoSalud)
        {
            try
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE animales SET estado_salud = {0} WHERE id = {1}",
                    estadoSalud,
                    animalId
                );
            }
            catch
            {
              
            }
        }
    }
}