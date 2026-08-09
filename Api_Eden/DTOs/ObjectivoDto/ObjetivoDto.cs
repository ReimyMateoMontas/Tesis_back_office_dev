namespace Api_Eden.DTOs.ObjectivoDto
{
    public record CrearObjetivoDto(
      string Nombre,
      string? Descripcion,
      decimal MontoObjetivo,
      string? FechaLimite,
      string? Observaciones
  );

    public record ActualizarObjetivoDto(
        string? Nombre,
        string? Descripcion,
        decimal? MontoObjetivo,
        string? Estado,
        string? FechaLimite,
        string? Observaciones
    );

    public record ObjetivoResponseDto(
        int Id,
        string Nombre,
        string? Descripcion,
        decimal MontoObjetivo,
        decimal MontoRecaudado,
        decimal Progreso,
        string Estado,
        string FechaInicio,
        string? FechaLimite,
        string CreadoPor,
        string? Observaciones,
        int TotalDonaciones
    );
}
