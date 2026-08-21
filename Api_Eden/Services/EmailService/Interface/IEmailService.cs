namespace Api_Eden.Services.EmailService.Interface
{
    // Ítem de vacuna para el correo de alerta.
    public record AlertaVacunaItem(string Animal, string TipoVacuna, string ProximaDosis, bool Vencida);

    public interface IEmailService
    {
        Task<bool> EnviarActivacionAsync(string destinatario, string nombre, string urlActivacion);
        Task<bool> EnviarRecuperacionAsync(string destinatario, string nombre, string urlReset);

        // Envía un resumen de las vacunas que tocan hoy / están vencidas.
        Task<bool> EnviarAlertaVacunaAsync(string destinatario, string nombre, IReadOnlyList<AlertaVacunaItem> vacunas);
    }
}
