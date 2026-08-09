using Api_Eden.Services.EmailService.Interface;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Api_Eden.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> EnviarActivacionAsync(
            string destinatario, string nombre, string urlActivacion)
        {
            var apiKey = GetSendGridApiKey();

            // ── Modo desarrollo: sin API key → solo loguear el link ───────────
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning(
                    "⚠ SendGrid no configurado. Link de activación para {Email}: {Url}",
                    destinatario, urlActivacion);

      
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"EMAIL DE ACTIVACIÓN (modo desarrollo)");
                Console.WriteLine($"Para: {destinatario}");
                Console.WriteLine($"Link: {urlActivacion}");
                Console.WriteLine($"========================================\n");

                return true; 
            }

            // ── Producción: envío real con SendGrid ───────────────────────────
            try
            {
                var remite = _config["SendGrid:From"] ?? "noreply@fundacioneden.com";
                var appName = _config["SendGrid:AppName"] ?? "Fundación El Edén";

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(remite, appName);
                var to = new EmailAddress(destinatario, nombre);
                var subject = $"Activa tu cuenta en {appName}";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
           background: #f9fafb; margin: 0; padding: 0; }}
    .container {{ max-width: 520px; margin: 40px auto; background: #ffffff;
                  border-radius: 16px; overflow: hidden; border: 1px solid #e5e7eb; }}
    .header {{ background: #16a34a; padding: 32px 40px; text-align: center; }}
    .header h1 {{ color: #ffffff; margin: 0; font-size: 22px; font-weight: 700; }}
    .header p  {{ color: #bbf7d0; margin: 6px 0 0; font-size: 14px; }}
    .body {{ padding: 36px 40px; }}
    .body h2 {{ color: #111827; font-size: 18px; margin: 0 0 12px; }}
    .body p  {{ color: #6b7280; font-size: 14px; line-height: 1.6; margin: 0 0 20px; }}
    .btn {{ display: inline-block; background: #16a34a; color: #ffffff !important;
            text-decoration: none; padding: 14px 32px; border-radius: 10px;
            font-weight: 600; font-size: 15px; }}
    .footer {{ background: #f9fafb; padding: 20px 40px; text-align: center;
               border-top: 1px solid #f3f4f6; }}
    .footer p {{ color: #9ca3af; font-size: 12px; margin: 0; }}
    .expiry {{ background: #fef9c3; border: 1px solid #fde68a; border-radius: 8px;
               padding: 12px 16px; margin: 20px 0; }}
    .expiry p {{ color: #92400e; font-size: 13px; margin: 0; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>🐾 {appName}</h1>
      <p>Sistema de Gestión del Albergue</p>
    </div>
    <div class='body'>
      <h2>Hola, {nombre}</h2>
      <p>El administrador ha creado una cuenta para ti en el sistema de gestión de {appName}.</p>
      <p>Haz clic en el botón para activar tu cuenta y crear tu contraseña:</p>
      <div style='text-align: center; margin: 28px 0;'>
        <a href='{urlActivacion}' class='btn'>Activar mi cuenta</a>
      </div>
      <div class='expiry'>
        <p>⏰ Este enlace expira en <strong>24 horas</strong>.</p>
      </div>
      <p>Si no esperabas este correo, puedes ignorarlo.</p>
    </div>
    <div class='footer'>
      <p>{appName} · Sistema de Gestión de Animales</p>
    </div>
  </div>
</body>
</html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);
                var response = await client.SendEmailAsync(msg);
                return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de activación a {Email}", destinatario);
                return false;
            }
        }
    

        public async Task<bool> EnviarRecuperacionAsync(
            string destinatario, string nombre, string urlReset)
        {
            var apiKey = GetSendGridApiKey();

            // ── Modo desarrollo ───────────────────────────────────────────────
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("⚠ Recuperación (dev). Link para {Email}: {Url}", destinatario, urlReset);
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"RECUPERAR CONTRASEÑA (modo desarrollo)");
                Console.WriteLine($"Para: {destinatario}");
                Console.WriteLine($"Link: {urlReset}");
                Console.WriteLine($"========================================\n");
                return true;
            }

            // ── Producción: SendGrid ──────────────────────────────────────────
            try
            {
                var remite = _config["SendGrid:From"] ?? "noreply@fundacioneden.com";
                var appName = _config["SendGrid:AppName"] ?? "Fundación El Edén";

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(remite, appName);
                var to = new EmailAddress(destinatario, nombre);
                var subject = $"Recupera tu contraseña en {appName}";

                var html = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; background:#f9fafb; margin:0; padding:0; }}
    .container {{ max-width:520px; margin:40px auto; background:#fff; border-radius:16px; border:1px solid #e5e7eb; overflow:hidden; }}
    .header {{ background:#16a34a; padding:32px 40px; text-align:center; }}
    .header h1 {{ color:#fff; margin:0; font-size:22px; font-weight:700; }}
    .header p {{ color:#bbf7d0; margin:6px 0 0; font-size:14px; }}
    .body {{ padding:36px 40px; }}
    .body h2 {{ color:#111827; font-size:18px; margin:0 0 12px; }}
    .body p {{ color:#6b7280; font-size:14px; line-height:1.6; margin:0 0 20px; }}
    .btn {{ display:inline-block; background:#16a34a; color:#fff !important; text-decoration:none;
            padding:14px 32px; border-radius:10px; font-weight:600; font-size:15px; }}
    .warning {{ background:#fef9c3; border:1px solid #fde68a; border-radius:8px; padding:12px 16px; margin:20px 0; }}
    .warning p {{ color:#92400e; font-size:13px; margin:0; }}
    .footer {{ background:#f9fafb; padding:20px 40px; text-align:center; border-top:1px solid #f3f4f6; }}
    .footer p {{ color:#9ca3af; font-size:12px; margin:0; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1>🐾 {appName}</h1>
      <p>Recuperación de contraseña</p>
    </div>
    <div class='body'>
      <h2>Hola, {nombre}</h2>
      <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
      <p>Haz clic en el botón para crear una nueva contraseña:</p>
      <div style='text-align:center; margin:28px 0;'>
        <a href='{urlReset}' class='btn'>Restablecer contraseña</a>
      </div>
      <div class='warning'>
        <p>⏰ Este enlace expira en <strong>1 hora</strong>.</p>
      </div>
      <p style='color:#6b7280; font-size:13px;'>Si no solicitaste este cambio, puedes ignorar este correo. Tu contraseña no será modificada.</p>
    </div>
    <div class='footer'>
      <p>{appName} · Sistema de Gestión de Animales</p>
    </div>
  </div>
</body>
</html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);
                var response = await client.SendEmailAsync(msg);
                return (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de recuperación a {Email}", destinatario);
                return false;
            }
        }

        private string? GetSendGridApiKey()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                ?? _config["SendGrid:ApiKey"];

            if (apiKey is "SENDGRID_API_KEY" or "TU_API_KEY_DE_SENDGRID")
            {
                return null;
            }

            return apiKey;
        }

    }
}
