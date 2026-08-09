using Api_Eden.Data;
using Api_Eden.DTOs.AuthDto;
using Api_Eden.Models;
using Api_Eden.Services.EmailService.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api_Eden.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext db, IConfiguration config, IEmailService emailService)
        {
            _db = db;
            _config = config;
            _emailService = emailService;
        }

        // ── POST /auth/Registro ───────────────────────────────────────────────

        [Authorize(Roles = "Administrador")]
        [HttpPost("Registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroDto dto)
        {
            try
            {
                if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest(new { mensaje = "Este Email ya existe." });

                // Generar token de activación seguro
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                        .Replace("+", "-").Replace("/", "_").Replace("=", "");
                var expiracion = DateTime.UtcNow.AddHours(24);

                var usuario = new Usuario
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Rol = dto.Rol,
                    Activo = false,          
                    EmailVerificado = false,
                    TokenActivacion = token,
                    TokenExpiracion = expiracion,
                    FechaCreacion = DateTime.UtcNow,
                    FechaUltimaModificacion = DateTime.UtcNow,
                };

                _db.Usuarios.Add(usuario);
                await _db.SaveChangesAsync();

              
                var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
                var urlActivacion = $"{frontendUrl}/activar?token={token}";

                var enviado = await _emailService.EnviarActivacionAsync(
                    dto.Email,
                    dto.Nombre,
                    urlActivacion
                );

                if (!enviado)
                {
                    // El usuario se creó pero el email falló — se puede reenviar luego
                    return Ok(new
                    {
                        mensaje = "Usuario creado pero no se pudo enviar el email de activación.",
                        emailEnviado = false
                    });
                }

                return Ok(new
                {
                    mensaje = $"Usuario creado. Se envió un enlace de activación a {dto.Email}.",
                    emailEnviado = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

      
        [AllowAnonymous]
        [HttpGet("activar")]
        public async Task<IActionResult> VerificarToken([FromQuery] string token)
        {
            try
            {
                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.TokenActivacion == token);

                if (usuario is null)
                    return BadRequest(new { mensaje = "El enlace de activación no es válido." });

                if (usuario.TokenExpiracion < DateTime.UtcNow)
                    return BadRequest(new { mensaje = "El enlace de activación ha expirado. Solicita uno nuevo al administrador." });

                if (usuario.EmailVerificado)
                    return Ok(new { mensaje = "La cuenta ya está activada.", yaActivo = true });

                return Ok(new
                {
                    mensaje = "Token válido.",
                    yaActivo = false,
                    nombre = usuario.Nombre,
                    email = usuario.Email,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al verificar el token", error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("activar")]
        public async Task<IActionResult> ActivarCuenta([FromBody] ActivarCuentaDto dto)
        {
            try
            {
                if (dto.Password.Length < 6)
                    return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

                if (dto.Password != dto.ConfirmarPassword)
                    return BadRequest(new { mensaje = "Las contraseñas no coinciden." });

                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.TokenActivacion == dto.Token);

                if (usuario is null)
                    return BadRequest(new { mensaje = "El enlace de activación no es válido." });

                if (usuario.TokenExpiracion < DateTime.UtcNow)
                    return BadRequest(new { mensaje = "El enlace ha expirado. Solicita uno nuevo al administrador." });

                if (usuario.EmailVerificado)
                    return BadRequest(new { mensaje = "Esta cuenta ya fue activada. Inicia sesión normalmente." });

              
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                usuario.EmailVerificado = true;
                usuario.Activo = true;
                usuario.TokenActivacion = null;  
                usuario.TokenExpiracion = null;
                usuario.FechaUltimaModificacion = DateTime.UtcNow;

                await _db.SaveChangesAsync();

              
                var jwt = GenerarToken(usuario);
                return Ok(new
                {
                    mensaje = "Cuenta activada correctamente. ¡Bienvenido!",
                    token = jwt,
                    usuario = new { usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al activar la cuenta", error = ex.Message });
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost("reenviar-activacion/{id}")]
        public async Task<IActionResult> ReenviarActivacion(int id)
        {
            try
            {
                var usuario = await _db.Usuarios.FindAsync(id);
                if (usuario is null)
                    return NotFound(new { mensaje = "Usuario no encontrado." });

                if (usuario.EmailVerificado)
                    return BadRequest(new { mensaje = "Este usuario ya tiene la cuenta activada." });

                // Generar nuevo token
                var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                        .Replace("+", "-").Replace("/", "_").Replace("=", "");
                var expiracion = DateTime.UtcNow.AddHours(24);

                usuario.TokenActivacion = token;
                usuario.TokenExpiracion = expiracion;
                usuario.FechaUltimaModificacion = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
                var urlActivacion = $"{frontendUrl}/activar?token={token}";

                var enviado = await _emailService.EnviarActivacionAsync(
                    usuario.Email, usuario.Nombre, urlActivacion);

                return Ok(new
                {
                    mensaje = enviado
                        ? $"Email de activación reenviado a {usuario.Email}."
                        : "Usuario actualizado. Revisa la consola para el link de activación.",
                    emailEnviado = enviado,
                  
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al reenviar activación", error = ex.Message });
            }
        }

        // ── POST /auth/Login 
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == loginDto.email);

     
                if (usuario == null)
                    return Unauthorized(new
                    {
                        mensaje = "El correo electrónico no está registrado en el sistema.",
                        codigo = "EMAIL_NOT_FOUND"
                    });

                if (!BCrypt.Net.BCrypt.Verify(loginDto.password, usuario.PasswordHash))
                    return Unauthorized(new
                    {
                        mensaje = "La contraseña es incorrecta. Verifica e intenta de nuevo.",
                        codigo = "WRONG_PASSWORD"
                    });

                if (usuario.Activo == false)
                    return Unauthorized(new
                    {
                        mensaje = "Tu cuenta está desactivada. Contacta al administrador.",
                        codigo = "ACCOUNT_INACTIVE"
                    });

             
                if (!usuario.EmailVerificado)
                    return Unauthorized(new
                    {
                        mensaje = "Debes activar tu cuenta primero. Revisa tu correo electrónico.",
                        codigo = "NOT_VERIFIED"
                    });

                var token = GenerarToken(usuario);
                return Ok(new
                {
                    Token = token,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Rol = usuario.Rol,
                    FotoPerfilUrl = usuario.FotoPerfilUrl,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error interno del servidor", error = ex.Message });
            }
        }

        // ── GET /auth/usuarios 
        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            try
            {
                var usuarios = await _db.Usuarios
                    .OrderBy(u => u.Rol).ThenBy(u => u.Nombre)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nombre,
                        u.Apellido,
                        u.Email,
                        u.Rol,
                        u.Activo,
                        u.EmailVerificado,
                        u.FotoPerfilUrl,
                        u.FechaCreacion,
                    })
                    .ToListAsync();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener usuarios", error = ex.Message });
            }
        }

        // ── PUT /auth/{id} 
        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });

            if (!string.IsNullOrWhiteSpace(dto.Nombre)) usuario.Nombre = dto.Nombre;
            if (!string.IsNullOrWhiteSpace(dto.Apellido)) usuario.Apellido = dto.Apellido;

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                    return BadRequest(new { mensaje = "El email ya está en uso." });
                usuario.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Rol))
            {
                var rolesValidos = new[] { "Administrador", "Veterinario", "Trabajador" };
                if (!rolesValidos.Contains(dto.Rol))
                    return BadRequest(new { mensaje = "Rol inválido." });
                usuario.Rol = dto.Rol;
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            if (dto.Activo.HasValue) usuario.Activo = dto.Activo;

           
            if (dto.FotoPerfilUrl != null)
                usuario.FotoPerfilUrl = string.IsNullOrWhiteSpace(dto.FotoPerfilUrl)
                    ? null
                    : dto.FotoPerfilUrl;

            usuario.FechaUltimaModificacion = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario actualizado correctamente." });
        }

        // ── DELETE /auth/{id} — soft delete 
        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario is null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id == idActual)
                return BadRequest(new { mensaje = "No puedes desactivar tu propio usuario." });

            usuario.Activo = false;
            usuario.FechaUltimaModificacion = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario desactivado correctamente." });
        }


        // ── POST /auth/recuperar-password ─────────────────────────────────────
      
        [AllowAnonymous]
        [HttpPost("recuperar-password")]
        public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordDto dto)
        {
            try
            {
                // Siempre devolver 200 por seguridad (no revelar si el email existe)
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (usuario != null && usuario.Activo == true && usuario.EmailVerificado)
                {
                    var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                            .Replace("+", "-").Replace("/", "_").Replace("=", "");
                    var expiracion = DateTime.UtcNow.AddHours(1);

                    // Reutilizamos los campos de activación para el reset
                    usuario.TokenActivacion = token;
                    usuario.TokenExpiracion = expiracion;
                    usuario.FechaUltimaModificacion = DateTime.UtcNow;
                    await _db.SaveChangesAsync();

                    var frontendUrl = _config["Frontend:Url"] ?? "http://localhost:5173";
                    var urlReset = $"{frontendUrl}/restablecer?token={token}";

                    await _emailService.EnviarRecuperacionAsync(usuario.Email, usuario.Nombre, urlReset);
                }

                return Ok(new { mensaje = "Si el correo está registrado, recibirás un enlace de recuperación." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al procesar la solicitud.", error = ex.Message });
            }
        }

        // ── GET /auth/verificar-reset?token=xxx ───────────────────────────────
     
        [AllowAnonymous]
        [HttpGet("verificar-reset")]
        public async Task<IActionResult> VerificarReset([FromQuery] string token)
        {
            try
            {
                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.TokenActivacion == token);

                if (usuario is null)
                    return BadRequest(new { mensaje = "El enlace de recuperación no es válido." });

                if (usuario.TokenExpiracion < DateTime.UtcNow)
                    return BadRequest(new { mensaje = "El enlace de recuperación ha expirado. Solicita uno nuevo." });

                return Ok(new { email = usuario.Email });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al verificar el token.", error = ex.Message });
            }
        }

        // ── POST /auth/restablecer-password ───────────────────────────────────
      
        [AllowAnonymous]
        [HttpPost("restablecer-password")]
        public async Task<IActionResult> RestablecerPassword([FromBody] ActivarCuentaDto dto)
        {
            try
            {
                if (dto.Password.Length < 6)
                    return BadRequest(new { mensaje = "La contraseña debe tener al menos 6 caracteres." });

                if (dto.Password != dto.ConfirmarPassword)
                    return BadRequest(new { mensaje = "Las contraseñas no coinciden." });

                var usuario = await _db.Usuarios
                    .FirstOrDefaultAsync(u => u.TokenActivacion == dto.Token);

                if (usuario is null)
                    return BadRequest(new { mensaje = "El enlace de recuperación no es válido." });

                if (usuario.TokenExpiracion < DateTime.UtcNow)
                    return BadRequest(new { mensaje = "El enlace ha expirado. Solicita uno nuevo." });

                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                usuario.TokenActivacion = null;
                usuario.TokenExpiracion = null;
                usuario.FechaUltimaModificacion = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                return Ok(new { mensaje = "Contraseña restablecida correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al restablecer la contraseña.", error = ex.Message });
            }
        }

        // ── Privado: GenerarToken ─────────────────────────────────────────────
        private string GenerarToken(Usuario usuario)
        {
            var jwtConfig = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email,          usuario.Email),
                new Claim(ClaimTypes.Name,           $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role,           usuario.Rol),
            };

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}