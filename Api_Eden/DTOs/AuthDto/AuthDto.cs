namespace Api_Eden.DTOs.AuthDto
{
    public record LoginDto(string email, string password);

    public record RegistroDto(
        string Nombre,
        string Apellido,
        string Email,
        string Rol = "Trabajador",
        string? Password = null,
        string? FotoPerfilUrl = null
    );

    public record AuthResponseDto(string Token, string Nombre, string Email, string Rol);

    public record RecuperarPasswordDto(string Email);
}