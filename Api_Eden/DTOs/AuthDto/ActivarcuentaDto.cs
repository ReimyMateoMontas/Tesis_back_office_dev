namespace Api_Eden.DTOs.AuthDto
{
    public record ActivarCuentaDto(
           string Token,
           string Password,
           string ConfirmarPassword
       );
}
