namespace Api_Eden.DTOs.DonacionesDto
{
    public record RegistrarDonacionDto(
         int TipoDonacionId,
         string FechaDonacion,
         int? DonanteId,
         string? NombreDonante,
         string? EmailDonante,
         string? TelefonoDonante,
         decimal? MontoDinero,
         decimal? ValorEstimado,
         int? CantidadArticulos,
         string? DescripcionDonacion,
         string? FormaPago,
         string? NumeroTransaccion,
         int? ObjetivoId,
         string? Observaciones
     );

    public record DonacionResponseDto(
        int Id,
        string TipoDonacion,
        int TipoDonacionId,
        bool EsMonetaria,
        decimal? MontoDinero,
        decimal? ValorEstimado,
        int? CantidadArticulos,
        string? DescripcionDonacion,
        string? FormaPago,
        string? NumeroTransaccion,
        string FechaDonacion,
        string Donante,
        int? DonanteId,
        string RegistradoPor,
        string? Observaciones,
        string? Objetivo,
        int? ObjetivoId,
        DateTime? FechaCreacion
    );

    public record ResumenDonacionesDto(
        decimal TotalDinero,
        decimal TotalEspecie,
        int TotalDonaciones,
        IEnumerable<CategoriaDonacionDto> PorTipo
    );

    public record CategoriaDonacionDto(string Tipo, decimal Total);
}
