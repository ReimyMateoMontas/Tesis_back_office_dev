using Api_Eden.Data;
using Api_Eden.Services.EmailService.Interface;
using Microsoft.EntityFrameworkCore;

namespace Api_Eden.Services.VacunaAlertaService
{
    /// <summary>
    /// Job en segundo plano que revisa periódicamente las vacunas cuya próxima dosis
    /// ya llegó (hoy o vencida) y siguen 'Pendiente', y envía un correo de recordatorio
    /// a administradores y veterinarios. Usa la columna `alerta_enviada` para no
    /// reenviar el mismo aviso (una sola alerta por vacuna).
    /// </summary>
    public class VacunaAlertaBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<VacunaAlertaBackgroundService> _logger;

        // Cada cuánto se revisa. 6 horas es suficiente para avisar el mismo día.
        private static readonly TimeSpan Intervalo = TimeSpan.FromHours(6);

        public VacunaAlertaBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<VacunaAlertaBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Pequeña espera inicial para no chocar con el arranque de la app.
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (TaskCanceledException) { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarYNotificarAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en el job de alertas de vacunas.");
                }

                try { await Task.Delay(Intervalo, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }

        private async Task RevisarYNotificarAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var email = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var hoy = DateOnly.FromDateTime(DateTime.Today);

            // Vacunas que tocan hoy / vencidas, pendientes y aún sin alerta enviada.
            var raw = await db.Vacunas
                .Where(v => (v.Estado == null || v.Estado == "Pendiente")
                            && v.ProximaDosis.HasValue
                            && v.ProximaDosis <= hoy
                            && v.AlertaEnviada == null)
                .Include(v => v.Animal)
                .Include(v => v.TipoVacuna)
                .OrderBy(v => v.ProximaDosis)
                .Select(v => new
                {
                    Animal = v.Animal.Nombre,
                    Tipo = v.TipoVacuna.Nombre,
                    Proxima = v.ProximaDosis!.Value
                })
                .ToListAsync(ct);

            if (raw.Count == 0)
                return;

            var pendientes = raw
                .Select(x => new AlertaVacunaItem(
                    x.Animal, x.Tipo, x.Proxima.ToString("yyyy-MM-dd"), x.Proxima < hoy))
                .ToList();

            // Destinatarios: administradores y veterinarios activos con correo.
            var destinatarios = await db.Usuarios
                .Where(u => u.Activo == true
                            && (u.Rol == "Administrador" || u.Rol == "Veterinario")
                            && u.Email != null && u.Email != "")
                .Select(u => new { u.Email, u.Nombre })
                .ToListAsync(ct);

            foreach (var d in destinatarios)
            {
                await email.EnviarAlertaVacunaAsync(d.Email, d.Nombre, pendientes);
            }

            // Marcar como alertadas (mismo predicado) para no reenviar.
            var afectadas = await db.Database.ExecuteSqlRawAsync(
                @"UPDATE vacunas
                     SET alerta_enviada = {0}
                   WHERE (estado = 'Pendiente' OR estado IS NULL)
                     AND proxima_dosis IS NOT NULL
                     AND proxima_dosis <= {0}
                     AND alerta_enviada IS NULL",
                hoy.ToString("yyyy-MM-dd"));

            _logger.LogInformation(
                "Alertas de vacunas: {Vacunas} vacuna(s) notificada(s) a {Destinatarios} destinatario(s). Filas marcadas: {Filas}.",
                pendientes.Count, destinatarios.Count, afectadas);
        }
    }
}
