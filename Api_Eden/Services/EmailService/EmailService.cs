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

                return !IsProduction();
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
                var responseBody = await response.Body.ReadAsStringAsync();
                var enviado = (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;

                if (enviado)
                {
                    _logger.LogInformation(
                        "SendGrid activación enviada a {Email}. StatusCode: {StatusCode}",
                        destinatario, response.StatusCode);
                }
                else
                {
                    _logger.LogError(
                        "SendGrid activación falló para {Email}. StatusCode: {StatusCode}. Body: {Body}",
                        destinatario, response.StatusCode, responseBody);
                }

                return enviado;
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
                return !IsProduction();
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
                var responseBody = await response.Body.ReadAsStringAsync();
                var enviado = (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;

                if (enviado)
                {
                    _logger.LogInformation(
                        "SendGrid recuperación enviada a {Email}. StatusCode: {StatusCode}",
                        destinatario, response.StatusCode);
                }
                else
                {
                    _logger.LogError(
                        "SendGrid recuperación falló para {Email}. StatusCode: {StatusCode}. Body: {Body}",
                        destinatario, response.StatusCode, responseBody);
                }

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar email de recuperación a {Email}", destinatario);
                return false;
            }
        }

        public async Task<bool> EnviarAlertaVacunaAsync(
            string destinatario, string nombre, IReadOnlyList<AlertaVacunaItem> vacunas)
        {
            if (vacunas is null || vacunas.Count == 0)
                return true; // nada que notificar

            var apiKey = GetSendGridApiKey();
            var appName = _config["SendGrid:AppName"] ?? "Fundación El Edén";

            // ── Modo desarrollo: sin API key → solo loguear ───────────────────
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning(
                    "⚠ SendGrid no configurado. Alerta de {Count} vacuna(s) para {Email}.",
                    vacunas.Count, destinatario);
                Console.WriteLine($"\n========================================");
                Console.WriteLine($"ALERTA DE VACUNAS (modo desarrollo)");
                Console.WriteLine($"Para: {destinatario}");
                foreach (var v in vacunas)
                    Console.WriteLine($"  - {v.Animal} · {v.TipoVacuna} · {v.ProximaDosis} {(v.Vencida ? "(VENCIDA)" : "(hoy)")}");
                Console.WriteLine($"========================================\n");
                return !IsProduction();
            }

            // ── Producción: envío real con SendGrid ───────────────────────────
            try
            {
                var remite = _config["SendGrid:From"] ?? "noreply@fundacioneden.com";

                var filas = string.Join("", vacunas.Select(v => $@"
      <tr>
        <td style='padding:10px 12px; border-bottom:1px solid #f3f4f6; font-size:14px; color:#111827;'>{v.Animal}</td>
        <td style='padding:10px 12px; border-bottom:1px solid #f3f4f6; font-size:14px; color:#6b7280;'>{v.TipoVacuna}</td>
        <td style='padding:10px 12px; border-bottom:1px solid #f3f4f6; font-size:14px; color:#6b7280;'>{v.ProximaDosis}</td>
        <td style='padding:10px 12px; border-bottom:1px solid #f3f4f6; font-size:13px; font-weight:600; color:{(v.Vencida ? "#dc2626" : "#d97706")};'>{(v.Vencida ? "Vencida" : "Hoy")}</td>
      </tr>"));

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(remite, appName);
                var to = new EmailAddress(destinatario, nombre);
                var subject = $"🐾 {vacunas.Count} vacuna(s) por aplicar — {appName}";

                var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family:-apple-system,BlinkMacSystemFont,Segoe UI,sans-serif; background:#f9fafb; margin:0; padding:0;'>
  <div style='max-width:560px; margin:40px auto; background:#fff; border-radius:16px; border:1px solid #e5e7eb; overflow:hidden;'>
    <div style='background:#16a34a; padding:28px 40px; text-align:center;'>
      <h1 style='color:#fff; margin:0; font-size:20px; font-weight:700;'>🐾 {appName}</h1>
      <p style='color:#bbf7d0; margin:6px 0 0; font-size:14px;'>Recordatorio de vacunación</p>
    </div>
    <div style='padding:32px 40px;'>
      <h2 style='color:#111827; font-size:17px; margin:0 0 12px;'>Hola, {nombre}</h2>
      <p style='color:#6b7280; font-size:14px; line-height:1.6; margin:0 0 20px;'>
        Las siguientes vacunas tienen su próxima dosis para <strong>hoy</strong> o ya están <strong>vencidas</strong>:
      </p>
      <table style='width:100%; border-collapse:collapse; border:1px solid #f3f4f6; border-radius:8px; overflow:hidden;'>
        <thead>
          <tr style='background:#f9fafb;'>
            <th style='padding:10px 12px; text-align:left; font-size:12px; text-transform:uppercase; color:#9ca3af;'>Animal</th>
            <th style='padding:10px 12px; text-align:left; font-size:12px; text-transform:uppercase; color:#9ca3af;'>Vacuna</th>
            <th style='padding:10px 12px; text-align:left; font-size:12px; text-transform:uppercase; color:#9ca3af;'>Próxima dosis</th>
            <th style='padding:10px 12px; text-align:left; font-size:12px; text-transform:uppercase; color:#9ca3af;'>Estado</th>
          </tr>
        </thead>
        <tbody>{filas}
        </tbody>
      </table>
      <p style='color:#9ca3af; font-size:12px; margin:20px 0 0;'>
        Ingresa al módulo Médico para registrar la dosis y marcar la vacuna como finalizada.
      </p>
    </div>
    <div style='background:#f9fafb; padding:18px 40px; text-align:center; border-top:1px solid #f3f4f6;'>
      <p style='color:#9ca3af; font-size:12px; margin:0;'>{appName} · Sistema de Gestión de Animales</p>
    </div>
  </div>
</body>
</html>";

                var msg = MailHelper.CreateSingleEmail(from, to, subject, "", html);
                var response = await client.SendEmailAsync(msg);
                var enviado = (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;

                if (enviado)
                    _logger.LogInformation("SendGrid alerta vacunas enviada a {Email}. StatusCode: {StatusCode}", destinatario, response.StatusCode);
                else
                    _logger.LogError("SendGrid alerta vacunas falló para {Email}. StatusCode: {StatusCode}", destinatario, response.StatusCode);

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar alerta de vacunas a {Email}", destinatario);
                return false;
            }
        }

        private string? GetSendGridApiKey()
        {
            var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY")
                ?? _config["SendGrid:ApiKey"];

            if (apiKey is "SENDGRID_API_KEY" or "TU_API_KEY_DE_SENDGRID" or "TU_SENDGRID_API_KEY")
            {
                return null;
            }

            return apiKey;
        }

        private bool IsProduction()
        {
            return string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Production",
                StringComparison.OrdinalIgnoreCase);
        }

    }
}
