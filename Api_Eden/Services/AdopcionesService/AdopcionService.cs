using Api_Eden.Data;
using Api_Eden.DTOs.AdopcionDto;
using Api_Eden.Services.AdopcionesService.Interface;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Microsoft.Extensions.Logging;
using System.Data;

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

            var connection = _db.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "SP_RegistrarAdopcion";

            command.Parameters.Add(new MySqlParameter("p_AnimalId", dto.AnimalId));
            command.Parameters.Add(new MySqlParameter("p_NombreAdoptante", dto.NombreAdoptante));
            command.Parameters.Add(new MySqlParameter("p_TelefonoAdoptante", (object?)dto.TelefonoAdoptante ?? DBNull.Value));
            command.Parameters.Add(new MySqlParameter("p_EmailAdoptante", (object?)dto.EmailAdoptante ?? DBNull.Value));
            command.Parameters.Add(new MySqlParameter("p_DireccionAdoptante", (object?)dto.DireccionAdoptante ?? DBNull.Value));
            command.Parameters.Add(new MySqlParameter("p_DocumentoIdentidad", (object?)dto.DocumentoIdentidad ?? DBNull.Value));
            command.Parameters.Add(new MySqlParameter("p_FechaAdopcion", dto.FechaAdopcion.ToString("yyyy-MM-dd")));
            command.Parameters.Add(new MySqlParameter("p_UsuarioId", dto.UsuarioResponsableId));

            var pResultado = new MySqlParameter("p_Resultado", MySqlDbType.VarChar)
            {
                Direction = System.Data.ParameterDirection.Output,
                Size = 100
            };
            command.Parameters.Add(pResultado);

            await command.ExecuteNonQueryAsync();

            var resultado = pResultado.Value?.ToString();
            if (resultado != null && resultado.StartsWith("ERROR"))
                return (false, resultado);

            return (true, resultado ?? "Adopción registrada correctamente.");
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
