using Api_Eden.Data;
using Api_Eden.DTOs.AdopcionDto;
using Api_Eden.Models;
using Api_Eden.Services.AdopcionesService.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api_Eden.Services.AdopcionesService
{
    public class AdopcionService : IAdopcionService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AdopcionService> _logger; 

        public AdopcionService(AppDbContext db, ILogger<AdopcionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<object> GetAdopciones()
        {
            try
            {
                return await _db.Adopciones
                    .Include(a => a.Animal)
                    .Include(a => a.UsuarioResponsable)
                    .OrderByDescending(a => a.FechaAdopcion)
                    .Select(a => new
                    {
                        a.Id,
                        Animal = a.Animal != null ? a.Animal.Nombre : "Sin información", 
                        a.NombreAdoptante,
                        a.TelefonoAdoptante,
                        a.EmailAdoptante,
                        a.FechaAdopcion,
                        a.EstadoAdopcion,
                        a.FechaSeguimiento,
                        UsuarioResponsable = a.UsuarioResponsable != null
                            ? $"{a.UsuarioResponsable.Nombre} {a.UsuarioResponsable.Apellido}"
                            : "Sin información",
                        a.Observaciones
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la lista de adopciones.");
                return new List<object>(); 
            }
        }

        public async Task<(bool ok, string mensaje, object? data)> GetAdopcion(int id)
        {
            try
            {
                var a = await _db.Adopciones
                    .Include(a => a.Animal)
                    .Include(a => a.UsuarioResponsable)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (a is null)
                    return (false, "Adopción no encontrada.", null);

                var data = new
                {
                    a.Id,
                    Animal = a.Animal?.Nombre ?? "Sin información",
                    a.NombreAdoptante,
                    a.TelefonoAdoptante,
                    a.EmailAdoptante,
                    a.DireccionAdoptante,
                    a.DocumentoIdentidad,
                    a.FechaAdopcion,
                    a.FechaSeguimiento,
                    a.EstadoAdopcion,
                    UsuarioResponsable = a.UsuarioResponsable != null
                            ? $"{a.UsuarioResponsable.Nombre} {a.UsuarioResponsable.Apellido}"
                            : "Sin información",
                    a.Observaciones
                };

                return (true, "OK", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener la adopción con ID {AdopcionId}.", id);
                return (false, "Ocurrió un error interno al consultar la adopción.", null);
            }
        }

        public async Task<(bool ok, string mensaje)> RegistrarAdopcion(RegistrarAdopcionDto dto)
        {
            var animal = await _db.Animales.FindAsync(dto.AnimalId);
            if (animal is null)
                return (false, "Animal no encontrado.");

            if (animal.EstadoGeneral == "Adoptado")
                return (false, "El animal ya fue adoptado.");

            if (animal.EstadoGeneral != "Activo")
                return (false, $"El animal no está disponible. Estado actual: {animal.EstadoGeneral}.");

            // Inserción directa con Entity Framework Core.
            // Antes se usaba el procedimiento almacenado SP_RegistrarAdopcion, que fallaba
            // en producción (MySQL sobre Linux, sensible a mayúsculas) porque referenciaba
            // la tabla "Adopciones" y en el servidor se llama "adopciones".
            // La adopción se crea en estado "Pendiente"; el animal se marca como "Adoptado"
            // solo cuando la adopción se aprueba (ver ActualizarEstado).
            var adopcion = new Adopcione
            {
                AnimalId = dto.AnimalId,
                NombreAdoptante = dto.NombreAdoptante,
                TelefonoAdoptante = dto.TelefonoAdoptante,
                EmailAdoptante = dto.EmailAdoptante,
                DireccionAdoptante = dto.DireccionAdoptante,
                DocumentoIdentidad = dto.DocumentoIdentidad,
                FechaAdopcion = dto.FechaAdopcion,
                EstadoAdopcion = "Pendiente",
                UsuarioResponsableId = dto.UsuarioResponsableId,
                FechaCreacion = DateTime.Now
            };

            _db.Adopciones.Add(adopcion);
            await _db.SaveChangesAsync();

            return (true, "Adopción registrada correctamente.");
        }

        public async Task<(bool ok, string mensaje)> ActualizarEstado(int id, ActualizarEstadoAdopcionDto dto)
        {
            try
            {
                var estadosValidos = new[] { "Pendiente", "Aprobada", "Rechazada", "Devuelto" };
                if (!estadosValidos.Contains(dto.Estado))
                    return (false, "Estado inválido. Usa: Pendiente, Aprobada, Rechazada o Devuelto.");

                var adopcion = await _db.Adopciones
                    .Include(a => a.Animal)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (adopcion is null)
                    return (false, "Adopción no encontrada.");

                var estadoAnterior = adopcion.EstadoAdopcion;
                adopcion.EstadoAdopcion = dto.Estado;

                if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                    adopcion.Observaciones = dto.Observaciones;

                if (adopcion.Animal != null)
                {
                    switch (dto.Estado)
                    {
                        case "Aprobada":
                            adopcion.Animal.EstadoGeneral = "Adoptado";
                            adopcion.Animal.FechaAdopcion = adopcion.FechaAdopcion;
                            break;   
                        case "Rechazada":
                        case "Devuelto":
                            adopcion.Animal.EstadoGeneral = "Activo";
                            adopcion.Animal.FechaAdopcion = null;
                            break;
                      
                        case "Pendiente":
                            if (estadoAnterior == "Aprobada")
                            {
                  
                                adopcion.Animal.EstadoGeneral = "Activo";
                                adopcion.Animal.FechaAdopcion = null;
                            }
                            break;
                    }
                }

                await _db.SaveChangesAsync();
                return (true, $"Estado de adopción actualizado a '{dto.Estado}'.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia al actualizar adopción {Id}.", id);
                return (false, "El registro fue modificado por otro usuario. Recarga e intenta de nuevo.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado de adopción {Id}.", id);
                return (false, "Error interno al actualizar el estado.");
            }
        }
    }
}
