namespace Api_Eden.DTOs.AuthDto
{
    public record ActualizarUsuarioDto(
     string? Nombre,
     string? Apellido,
     string? Email,
     string? Password,
     string? Rol,
     bool? Activo,
     string? FotoPerfilUrl
 );
}
