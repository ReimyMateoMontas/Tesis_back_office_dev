namespace Api_Eden.Services.EmailService.Interface
{

    public interface IEmailService
    {
        Task<bool> EnviarActivacionAsync(string destinatario, string nombre, string urlActivacion);
        Task<bool> EnviarRecuperacionAsync(string destinatario, string nombre, string urlReset);
    }

}
