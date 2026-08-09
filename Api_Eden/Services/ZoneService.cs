namespace Api_Eden.Services;

using Api_Eden.Data;
using Api_Eden.DTOs.Zone.Request;
using Api_Eden.DTOs.Zone.Response;
using Api_Eden.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class ZoneService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ZoneService> _logger;

    public ZoneService(AppDbContext context, ILogger<ZoneService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ZoneResponseDto>> GetAllAsync()
    {
        return await _context.Zonas
            .Select(z => new ZoneResponseDto(
                z.Id,
                z.Nombre,
                z.Descripcion,
                z.CapacidadMaxima,
                z.Animales.Count(a => a.EstadoGeneral == "Activo"),
                z.Activa,
                z.FechaCreacion
            ))
            .ToListAsync();
    }

    public async Task<ZoneResponseDto?> GetByIdAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser mayor a 0.");

        var zona = await _context.Zonas
            .Where(z => z.Id == id)
            .Select(z => new ZoneResponseDto(
                z.Id,
                z.Nombre,
                z.Descripcion,
                z.CapacidadMaxima,
                z.Animales.Count(a => a.EstadoGeneral == "Activo"),
                z.Activa,
                z.FechaCreacion
            ))
            .FirstOrDefaultAsync();

        if (zona is null) throw new KeyNotFoundException($"No existe una zona con ID {id}.");
        return zona;
    }

    
    public async Task<IEnumerable<object>> GetAnimalesByZonaAsync(int zonaId)
    {
        var zona = await _context.Zonas.FindAsync(zonaId)
            ?? throw new KeyNotFoundException($"No existe una zona con ID {zonaId}.");

        return await _context.Animales
            .Include(a => a.Especie)
            .Where(a => a.ZonaActualId == zonaId && a.EstadoGeneral == "Activo")
            .Select(a => new
            {
                a.Id,
                a.Nombre,
                Especie = a.Especie != null ? a.Especie.Nombre : null,
                a.Raza,
                a.Edad,
                a.Sexo,
                a.FotografiaUrl,
                a.EstadoSalud,
                a.EstadoGeneral,
            })
            .ToListAsync();
    }

    

   
    public async Task<IEnumerable<object>> GetMovimientosAsync()
    {
        return await _context.Historialmovimientos
            .Include(m => m.Animal)
            .Include(m => m.ZonaOrigen)
            .Include(m => m.ZonaDestino)
            .Include(m => m.UsuarioResponsable)
            .OrderByDescending(m => m.Fecha)
            .Select(m => new
            {
                m.Id,
                Animal = m.Animal.Nombre,
                AnimalId = m.AnimalId,
                ZonaOrigen = m.ZonaOrigen != null ? m.ZonaOrigen.Nombre : "—",
                ZonaDestino = m.ZonaDestino != null ? m.ZonaDestino.Nombre : "—",
                m.Motivo,
                m.Observaciones,
                Fecha = m.Fecha,
                Usuario = m.UsuarioResponsable.Nombre + " " + m.UsuarioResponsable.Apellido,
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<object>> GetMovimientosByAnimalAsync(int animalId)
    {
        return await _context.Historialmovimientos
            .Include(m => m.ZonaOrigen)
            .Include(m => m.ZonaDestino)
            .Include(m => m.UsuarioResponsable)
            .Where(m => m.AnimalId == animalId)
            .OrderByDescending(m => m.Fecha)
            .Select(m => new
            {
                m.Id,
                ZonaOrigen = m.ZonaOrigen != null ? m.ZonaOrigen.Nombre : "—",
                ZonaDestino = m.ZonaDestino != null ? m.ZonaDestino.Nombre : "—",
                m.Motivo,
                Fecha = m.Fecha,
                Usuario = m.UsuarioResponsable.Nombre + " " + m.UsuarioResponsable.Apellido,
            })
            .ToListAsync();
    }

    public async Task MoverAnimalAsync(MoverAnimalDto dto, int usuarioId)
    {
        var animal = await _context.Animales.FindAsync(dto.AnimalId)
            ?? throw new KeyNotFoundException($"No existe un animal con ID {dto.AnimalId}.");

        if (animal.EstadoGeneral != "Activo")
            throw new InvalidOperationException("Solo se pueden mover animales con estado Activo.");

        if (animal.ZonaActualId == dto.ZonaDestinoId)
            throw new InvalidOperationException("El animal ya se encuentra en esa zona.");

        var zonaDestino = await _context.Zonas.FindAsync(dto.ZonaDestinoId)
            ?? throw new KeyNotFoundException($"No existe una zona con ID {dto.ZonaDestinoId}.");

        if (zonaDestino.Activa == false)
            throw new InvalidOperationException($"La zona '{zonaDestino.Nombre}' no está activa.");

        var capacidadActual = zonaDestino.CantidadActual ?? 0;
        if (capacidadActual >= zonaDestino.CapacidadMaxima)
            throw new InvalidOperationException(
                $"La zona '{zonaDestino.Nombre}' está llena ({zonaDestino.CapacidadMaxima}/{zonaDestino.CapacidadMaxima}).");

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            int? zonaOrigenId = animal.ZonaActualId;

            
            if (zonaOrigenId.HasValue)
            {
                var zonaOrigen = await _context.Zonas.FindAsync(zonaOrigenId.Value);
                if (zonaOrigen != null && zonaOrigen.CantidadActual > 0)
                    zonaOrigen.CantidadActual--;
            }

            zonaDestino.CantidadActual = capacidadActual + 1;

            animal.ZonaActualId = dto.ZonaDestinoId;

            // Registrar movimiento
            var movimiento = new Historialmovimiento
            {
                AnimalId = dto.AnimalId,
                ZonaOrigenId = zonaOrigenId,
                ZonaDestinoId = dto.ZonaDestinoId,
                Motivo = dto.Motivo,
                Observaciones = dto.Observaciones,
                Fecha = DateTime.Now,
                UsuarioResponsableId = usuarioId,
            };

            _context.Historialmovimientos.Add(movimiento);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Animal {AnimalId} movido de zona {Origen} a zona {Destino} por usuario {UsuarioId}",
                dto.AnimalId, zonaOrigenId, dto.ZonaDestinoId, usuarioId);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ZoneResponseDto> CreateAsync(CreateZoneDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la zona es obligatorio.");
        if (dto.Nombre.Length > 100)
            throw new ArgumentException("El nombre no puede superar los 100 caracteres.");
        if (dto.CapacidadMaxima <= 0)
            throw new ArgumentException("La capacidad máxima debe ser mayor a 0.");
        if (await _context.Zonas.AnyAsync(z => z.Nombre == dto.Nombre))
            throw new ArgumentException($"Ya existe una zona con el nombre '{dto.Nombre}'.");

        var newZone = dto.Adapt<Zona>();
        newZone.FechaCreacion = DateTime.Now;
        newZone.CantidadActual = 0;
        newZone.Activa = true;

        _context.Zonas.Add(newZone);
        await _context.SaveChangesAsync();
        return newZone.Adapt<ZoneResponseDto>();
    }

    public async Task<bool> UpdateAsync(int id, CreateZoneDto dto)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(dto.Nombre)) throw new ArgumentException("El nombre de la zona es obligatorio.");
        if (dto.CapacidadMaxima <= 0) throw new ArgumentException("La capacidad máxima debe ser mayor a 0.");

        var zona = await _context.Zonas.FirstOrDefaultAsync(z => z.Id == id);
        if (zona is null) return false;

        if (await _context.Zonas.AnyAsync(z => z.Nombre == dto.Nombre && z.Id != id))
            throw new ArgumentException($"Ya existe otra zona con el nombre '{dto.Nombre}'.");
        if (dto.CapacidadMaxima < zona.CantidadActual)
            throw new ArgumentException(
                $"La capacidad máxima ({dto.CapacidadMaxima}) no puede ser menor a la cantidad actual ({zona.CantidadActual}).");

        dto.Adapt(zona);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0) throw new ArgumentException("El ID debe ser mayor a 0.");

        var zona = await _context.Zonas.Include(z => z.Animales).FirstOrDefaultAsync(z => z.Id == id);
        if (zona is null) return false;

        if (zona.Animales.Any(a => a.EstadoGeneral == "Activo"))
            throw new InvalidOperationException(
                $"No se puede eliminar '{zona.Nombre}' porque tiene {zona.CantidadActual} animal(es) activo(s).");

        zona.Activa = false;
        await _context.SaveChangesAsync();
        return true;
    }
}