
using Api_Eden.Data;
using Api_Eden.DTOs.ObjectivoDto;

using Api_Eden.Models;
using Api_Eden.Services.ObjetivoService.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.ObjetivoService
{
    public class ObjetivoService : IObjetivoService
    {
        private readonly AppDbContext _db;

        public ObjetivoService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<ObjetivoResponseDto>> GetAllAsync()
        {
            var objetivos = await _db.Objetivos
                .Include(o => o.UsuarioCreo)
                .OrderByDescending(o => o.FechaCreacion)
                .ToListAsync();

            return objetivos.Select(o => new ObjetivoResponseDto(
                o.Id,
                o.Nombre,
                o.Descripcion,
                o.MontoObjetivo,
                o.MontoRecaudado,
                o.MontoObjetivo > 0
                    ? Math.Round((o.MontoRecaudado / o.MontoObjetivo) * 100, 1)
                    : 0m,
                o.Estado,
                o.FechaInicio.ToString("yyyy-MM-dd"),
                o.FechaLimite?.ToString("yyyy-MM-dd"),
                $"{o.UsuarioCreo.Nombre} {o.UsuarioCreo.Apellido}",
                o.Observaciones,
                _db.Donaciones.Count(d => d.ObjetivoId == o.Id)
            ));
        }

        public async Task<(bool ok, string mensaje, int? id)> CrearAsync(
            CrearObjetivoDto dto, int usuarioId)
        {
            if (dto.MontoObjetivo <= 0)
                return (false, "El monto objetivo debe ser mayor a 0.", null);

            var objetivo = new Objetivo
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion,
                MontoObjetivo = dto.MontoObjetivo,
                MontoRecaudado = 0,
                Estado = "Activo",
                FechaInicio = DateOnly.FromDateTime(DateTime.Today),
                FechaLimite = TryParseDate(dto.FechaLimite),
                UsuarioCreoId = usuarioId,
                Observaciones = dto.Observaciones,
                FechaCreacion = DateTime.UtcNow,
            };

            _db.Objetivos.Add(objetivo);
            await _db.SaveChangesAsync();
            return (true, "Objetivo creado correctamente.", objetivo.Id);
        }

        public async Task<(bool ok, string mensaje)> ActualizarAsync(
            int id, ActualizarObjetivoDto dto)
        {
            var objetivo = await _db.Objetivos.FindAsync(id);
            if (objetivo is null) return (false, "Objetivo no encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) objetivo.Nombre = dto.Nombre.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Descripcion)) objetivo.Descripcion = dto.Descripcion;
            if (dto.MontoObjetivo.HasValue && dto.MontoObjetivo > 0)
                objetivo.MontoObjetivo = dto.MontoObjetivo.Value;
            if (!string.IsNullOrWhiteSpace(dto.Estado)) objetivo.Estado = dto.Estado;
            if (!string.IsNullOrWhiteSpace(dto.FechaLimite))
            {
                var fecha = TryParseDate(dto.FechaLimite);
                if (fecha.HasValue) objetivo.FechaLimite = fecha;
            }
            if (!string.IsNullOrWhiteSpace(dto.Observaciones)) objetivo.Observaciones = dto.Observaciones;

            // Auto-completar si alcanzó el monto
            if (objetivo.MontoRecaudado >= objetivo.MontoObjetivo && objetivo.Estado == "Activo")
                objetivo.Estado = "Completado";

            objetivo.FechaActualizacion = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (true, "Objetivo actualizado correctamente.");
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var objetivo = await _db.Objetivos.FindAsync(id);
            if (objetivo is null) return (false, "Objetivo no encontrado.");

            // Desligar donaciones
            var donaciones = await _db.Donaciones.Where(d => d.ObjetivoId == id).ToListAsync();
            donaciones.ForEach(d => d.ObjetivoId = null);

            _db.Objetivos.Remove(objetivo);
            await _db.SaveChangesAsync();
            return (true, "Objetivo eliminado correctamente.");
        }
      
        private static DateOnly? TryParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return DateOnly.TryParse(value, out var date) ? date : null;
        }
    }
}