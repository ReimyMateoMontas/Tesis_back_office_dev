using Api_Eden.Data;
using Api_Eden.DTOs.AuthDto;
using Api_Eden.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api_Eden.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<(bool exito, string mensaje)> RegistrarAsync(RegistroDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return (false, "Este Email ya existe");

        var usuario = new Usuario
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Rol = dto.Rol,
            Activo = true,
            FechaCreacion = DateTime.UtcNow,
            FechaUltimaModificacion = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();
        return (true, "Usuario registrado exitosamente");
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.email);

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.password, usuario.PasswordHash))
            return null;

        var token = GenerarToken(usuario);
        return new AuthResponseDto(token, usuario.Nombre, usuario.Email, usuario.Rol);
    }

    public async Task<(bool exito, string mensaje)> ActualizarAsync(int id, ActualizarUsuarioDto dto)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null)
            return (false, "Usuario no encontrado");

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
            usuario.Nombre = dto.Nombre;

        if (!string.IsNullOrWhiteSpace(dto.Apellido))
            usuario.Apellido = dto.Apellido;

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email && u.Id != id))
                return (false, "El email ya está en uso por otro usuario");
            usuario.Email = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.Rol))
        {
            var rolesValidos = new[] { "Administrador", "Veterinario", "Trabajador" };
            if (!rolesValidos.Contains(dto.Rol))
                return (false, "Rol inválido. Usa: Administrador, Veterinario o Trabajador");
            usuario.Rol = dto.Rol;
        }

        if (!string.IsNullOrWhiteSpace(dto.Password))
            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        if (dto.Activo.HasValue)
            usuario.Activo = dto.Activo;

        usuario.FechaUltimaModificacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Usuario actualizado correctamente");
    }

    public async Task<(bool exito, string mensaje)> EliminarAsync(int id, int idActual)
    {
        if (id == idActual)
            return (false, "No puedes eliminar tu propio usuario");

        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario is null)
            return (false, "Usuario no encontrado");

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();
        return (true, "Usuario eliminado correctamente");
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email,           usuario.Email),
            new Claim(ClaimTypes.Name,            $"{usuario.Nombre} {usuario.Apellido}"),
            new Claim(ClaimTypes.Role,            usuario.Rol)
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}