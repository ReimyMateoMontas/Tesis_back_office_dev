using Api_Eden.Data;
using Api_Eden.Services.Dashboard.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _db;

        public DashboardService(AppDbContext db) => _db = db;

        public async Task<DashboardResumenDto> GetResumenAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);
            var hace7Dias = hoy.AddDays(-7);
            var inicioMes = new DateOnly(hoy.Year, hoy.Month, 1);

           
            var stats = await GetStatsAsync(hace7Dias);
            var zonas = await GetZonasAsync();
            var gastosMensuales = await GetGastosMensualesAsync(inicioMes);
            var gastosPorCategoria = await GetGastosPorCategoriaAsync(inicioMes);
            var actividadReciente = await GetActividadRecienteAsync();

            return new DashboardResumenDto(
                stats,
                GetEstadoSalud(stats).ToList(), 
                zonas,
                gastosMensuales,
                gastosPorCategoria,
                actividadReciente
            );
        }

        private async Task<DashboardStatsDto> GetStatsAsync(DateOnly hace7dias)
        {
            // Nota: Si el constructor falla, verifica el orden
                return new DashboardStatsDto(
                await _db.Animales.CountAsync(a => a.EstadoGeneral == "Activo"),
                await _db.Animales.CountAsync(a => a.EstadoSalud == "Saludable" && a.EstadoGeneral == "Activo"),
                await _db.Animales.CountAsync(a => a.EstadoSalud == "EnTratamiento" && a.EstadoGeneral == "Activo"),
                await _db.Animales.CountAsync(a => a.EstadoSalud == "Critico" && a.EstadoGeneral == "Activo"),
                await _db.Animales.CountAsync(a => a.EstadoSalud == "Recuperado" && a.EstadoGeneral == "Activo"),
                await _db.Animales.CountAsync(a => a.EstadoGeneral == "Adoptado"),
                await _db.Animales.CountAsync(a => a.FechaIngreso >= hace7dias),
                await _db.Adopciones.CountAsync(a => a.EstadoAdopcion == "Pendiente"),
                await _db.Tratamientos.CountAsync(t => t.Estado == "Activo"),
                await _db.Alimentos.CountAsync(a => a.Activo == true && a.CantidadDisponible <= a.StockMinimo),
                await _db.Vacunas.CountAsync(v =>
                    (v.Estado == null || v.Estado == "Pendiente")
                    && v.ProximaDosis.HasValue
                    && v.ProximaDosis <= DateOnly.FromDateTime(DateTime.Today))
            );
        }

        private static IEnumerable<EstadoSaludDto> GetEstadoSalud(DashboardStatsDto stats) =>
        [
            new("Saludable",      stats.Saludables),
            new("En tratamiento", stats.EnTratamiento),
            new("Recuperado",     stats.Recuperados),
            new("Crítico",        stats.Criticos),
        ];

        private async Task<IEnumerable<ZonaOcupacionDto>> GetZonasAsync()
        {
            return await _db.Zonas
                .Select(z => new ZonaOcupacionDto(
                    z.Nombre,
                    z.CapacidadMaxima,
                    _db.Animales.Count(a => a.ZonaActualId == z.Id && a.EstadoGeneral == "Activo")
                ))
                .ToListAsync();
        }

        private async Task<IEnumerable<GastoMensualDto>> GetGastosMensualesAsync(DateOnly inicioMes)
        {
            var fechaLimite = inicioMes.AddMonths(-5);

            var data = await _db.Gastos
                .Where(g => g.FechaGasto >= fechaLimite)
                .Select(g => new { g.FechaGasto, g.Monto }) 
                .ToListAsync();

            return data
                .GroupBy(g => new { g.FechaGasto.Year, g.FechaGasto.Month })
                .Select(g => new GastoMensualDto(
                    g.Key.Year,
                    g.Key.Month,
                    g.Sum(x => x.Monto)
                ))
                .OrderBy(g => g.Anio)
                .ThenBy(g => g.Mes)
                .ToList();
        }

        private async Task<IEnumerable<GastoCategoriaDto>> GetGastosPorCategoriaAsync(DateOnly inicioMes)
        {
            var data = await _db.Gastos
                .Where(g => g.FechaGasto >= inicioMes)
                .Select(g => new {
                    Categoria = g.CategoriaGasto != null ? g.CategoriaGasto.Nombre : "Sin Categoría",
                    g.Monto
                })
                .ToListAsync();

            return data
                .GroupBy(g => g.Categoria)
                .Select(g => new GastoCategoriaDto(g.Key, g.Sum(x => x.Monto)))
                .OrderByDescending(g => g.Total)
                .ToList();
        }

        private async Task<IEnumerable<ActividadDto>> GetActividadRecienteAsync()
        {
            var actividad = new List<ActividadDto>();

            // 1. Animales (Optimizado con Select)
            var ultimosAnimales = await _db.Animales
                .OrderByDescending(a => a.FechaCreacion)
                .Take(3)
                .Select(a => new { a.Nombre, Especie = a.Especie.Nombre, a.Raza, a.FechaCreacion })
                .ToListAsync();

            actividad.AddRange(ultimosAnimales.Select(a => new ActividadDto(
                "animal",
                $"Nuevo ingreso: {a.Nombre}",
                $"{a.Especie ?? "Animal"}{(a.Raza != null ? $" · {a.Raza}" : "")} admitido",
                a.FechaCreacion
            )));

            // 2. Tratamientos
            var ultimosTratamientos = await _db.Tratamientos
                .Where(t => t.Estado == "Completado")
                .OrderByDescending(t => t.FechaFin)
                .Take(2)
                .Select(t => new { AnimalNombre = t.HistorialMedico.Animal.Nombre, t.FechaFin })
                .ToListAsync();

            actividad.AddRange(ultimosTratamientos.Select(t => new ActividadDto(
                "tratamiento",
                "Tratamiento completado",
                $"{t.AnimalNombre} completó su tratamiento",
                t.FechaFin?.ToDateTime(TimeOnly.MinValue)
            )));

            // 3. Stock
            var alimentosBajos = await _db.Alimentos
                .Where(a => a.Activo == true && a.CantidadDisponible <= a.StockMinimo)
                .OrderByDescending(a => a.FechaCreacion)
                .Take(2)
                .Select(al => new { al.Nombre, al.FechaCreacion })
                .ToListAsync();

            actividad.AddRange(alimentosBajos.Select(al => new ActividadDto(
                "stock",
                "Alerta de stock",
                $"{al.Nombre} por debajo del mínimo",
                al.FechaCreacion
            )));

            // 4. Adopciones
            var ultimasAdopciones = await _db.Adopciones
                .Where(a => a.EstadoAdopcion == "Aprobada")
                .OrderByDescending(a => a.FechaAdopcion)
                .Take(2)
                .Select(ad => new { ad.NombreAdoptante, ad.FechaAdopcion })
                .ToListAsync();

            actividad.AddRange(ultimasAdopciones.Select(ad => new ActividadDto(
                "adopcion",
                "Adopción aprobada",
                $"{ad.NombreAdoptante} adoptó un animal",
                ad.FechaAdopcion.ToDateTime(TimeOnly.MinValue)
            )));

            return actividad
                .OrderByDescending(a => a.Fecha ?? DateTime.MinValue)
                .Take(8)
                .ToList();
        }
    }
}