namespace Api_Eden.DTOs.AdopcionDto
{
  
    public record RegistrarAdopcionDto(
        int AnimalId,
        string NombreAdoptante,
        string? TelefonoAdoptante,
        string? EmailAdoptante,
        string? DireccionAdoptante,
        string? DocumentoIdentidad,
        DateOnly FechaAdopcion,
        int UsuarioResponsableId
    );

    public record ActualizarEstadoAdopcionDto(
        string Estado,        // Pendiente, Aprobada, Rechazada, Devuelto
        string? Observaciones
    );
}
