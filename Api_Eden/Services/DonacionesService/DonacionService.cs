

using Api_Eden.Data;

using Api_Eden.DTOs.DonacionesDto;
using Api_Eden.Models;
using Api_Eden.Services.DonacionesService.Interface;

using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.DonacionService
{
    public class DonacionService : IDonacionService
    {
        private readonly AppDbContext _db;

        public DonacionService(AppDbContext db) => _db = db;

        public async Task<IEnumerable<DonacionResponseDto>> GetAllAsync()
        {
            var donaciones = await _db.Donaciones
                .Include(d => d.TipoDonacion)
                .Include(d => d.Donante)
                .Include(d => d.UsuarioRegistro)
                .Include(d => d.Objetivo)
                .OrderByDescending(d => d.FechaDonacion)
                .ToListAsync();

            return donaciones.Select(d => new DonacionResponseDto(
                d.Id,
                d.TipoDonacion.Nombre,
                d.TipoDonacionId,
                d.MontoDinero != null && d.MontoDinero > 0,
                d.MontoDinero,
                d.ValorEstimado,
                d.CantidadArticulos,
                d.DescripcionDonacion,
                d.FormaPago,
                d.NumeroTransaccion,
                d.FechaDonacion.ToString("yyyy-MM-dd"),
                d.Donante != null ? d.Donante.Nombre : "Anónimo",
                d.DonanteId,
                $"{d.UsuarioRegistro.Nombre} {d.UsuarioRegistro.Apellido}",
                d.Observaciones,
                d.Objetivo?.Nombre,
                d.ObjetivoId,
                d.FechaCreacion
            ));
        }

        public async Task<ResumenDonacionesDto> GetResumenAsync()
        {
            var totalDinero = await _db.Donaciones.SumAsync(d => d.MontoDinero ?? 0);
            var totalEspecie = await _db.Donaciones.SumAsync(d => d.ValorEstimado ?? 0);
            var totalDonaciones = await _db.Donaciones.CountAsync();

            var raw = await _db.Donaciones
                .Include(d => d.TipoDonacion)
                .ToListAsync();

            var porTipo = raw
                .GroupBy(d => d.TipoDonacion.Nombre)
                .Select(g => new CategoriaDonacionDto(
                    g.Key,
                    g.Sum(d => (d.MontoDinero ?? 0) + (d.ValorEstimado ?? 0))
                ))
                .ToList();

            return new ResumenDonacionesDto(totalDinero, totalEspecie, totalDonaciones, porTipo);
        }

        public async Task<(bool ok, string mensaje, int? id)> RegistrarAsync(
            RegistrarDonacionDto dto, int usuarioId)
        {
            try
            {
                // Crear donante nuevo si es necesario
                int? donanteId = dto.DonanteId;
                if (donanteId == null && !string.IsNullOrWhiteSpace(dto.NombreDonante))
                {
                    var nuevoDonante = new Donante
                    {
                        Nombre = dto.NombreDonante.Trim(),
                        TipoDonante = "Persona",
                        Email = dto.EmailDonante,
                        Telefono = dto.TelefonoDonante,
                        FechaRegistro = DateOnly.FromDateTime(DateTime.Today),
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow,
                    };
                    _db.Donantes.Add(nuevoDonante);
                    await _db.SaveChangesAsync();
                    donanteId = nuevoDonante.Id;
                }

                var donacion = new Donacione
                {
                    DonanteId = donanteId,
                    TipoDonacionId = dto.TipoDonacionId,
                    MontoDinero = dto.MontoDinero,
                    ValorEstimado = dto.ValorEstimado,
                    CantidadArticulos = dto.CantidadArticulos,
                    DescripcionDonacion = dto.DescripcionDonacion,
                    FormaPago = dto.FormaPago,
                    NumeroTransaccion = dto.NumeroTransaccion,
                    FechaDonacion = DateOnly.TryParse(dto.FechaDonacion, out var fd)
                                        ? fd
                                        : DateOnly.FromDateTime(DateTime.Today),
                    ObjetivoId = dto.ObjetivoId,
                    Observaciones = dto.Observaciones,
                    UsuarioRegistroId = usuarioId,
                    FechaCreacion = DateTime.UtcNow,
                };

                _db.Donaciones.Add(donacion);

                // Actualizar objetivo si aplica
                if (dto.ObjetivoId.HasValue && dto.MontoDinero.HasValue && dto.MontoDinero > 0)
                {
                    var objetivo = await _db.Objetivos.FindAsync(dto.ObjetivoId.Value);
                    if (objetivo != null)
                    {
                        objetivo.MontoRecaudado += dto.MontoDinero.Value;
                        if (objetivo.MontoRecaudado >= objetivo.MontoObjetivo)
                            objetivo.Estado = "Completado";
                        objetivo.FechaActualizacion = DateTime.UtcNow;
                    }
                }

                await _db.SaveChangesAsync();
                return (true, "Donación registrada correctamente.", donacion.Id);
            }
            catch (Exception ex)
            {
                return (false, $"Error interno: {ex.Message}", null);
            }
        }

        public async Task<(bool ok, string mensaje)> EliminarAsync(int id)
        {
            var donacion = await _db.Donaciones.FindAsync(id);
            if (donacion is null) return (false, "Donación no encontrada.");

            if (donacion.ObjetivoId.HasValue && donacion.MontoDinero.HasValue)
            {
                var objetivo = await _db.Objetivos.FindAsync(donacion.ObjetivoId.Value);
                if (objetivo != null)
                {
                    objetivo.MontoRecaudado = Math.Max(0, objetivo.MontoRecaudado - donacion.MontoDinero.Value);
                    if (objetivo.Estado == "Completado" && objetivo.MontoRecaudado < objetivo.MontoObjetivo)
                        objetivo.Estado = "Activo";
                    objetivo.FechaActualizacion = DateTime.UtcNow;
                }
            }

            _db.Donaciones.Remove(donacion);
            await _db.SaveChangesAsync();
            return (true, "Donación eliminada correctamente.");
        }

        public async Task<IEnumerable<object>> GetTiposAsync() =>
            await _db.Tiposdonacions
                .Where(t => t.Activa == true)
                .OrderBy(t => t.Nombre)
                .Select(t => new { t.Id, t.Nombre })
                .ToListAsync();

        public async Task<IEnumerable<object>> GetDonantesAsync() =>
            await _db.Donantes
                .Where(d => d.Activo == true) 
                .OrderBy(d => d.Nombre)
                .Select(d => new { d.Id, d.Nombre, d.Email, d.Telefono })
                .ToListAsync();
    }
}