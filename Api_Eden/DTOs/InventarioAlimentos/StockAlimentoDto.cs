namespace Api_Eden.DTOs.InventarioAlimentos
{
    public record AlimentoDto(
        int Id,
        string Nombre,
        string TipoAnimal,
        string? Marca,
        string UnidadMedida,
        decimal CantidadDisponible,
        decimal StockMinimo,
        DateOnly? FechaVencimiento,
        bool? Activo,
        bool StockBajo
    );

    public record CrearAlimentoDto(
        string Nombre,
        string TipoAnimal,
        string? Marca,
        string UnidadMedida,
        decimal CantidadDisponible,
        decimal StockMinimo,
        DateOnly? FechaVencimiento
    );

    public record ActualizarAlimentoDto(
        string? Nombre,
        string? TipoAnimal,
        string? Marca,
        string? UnidadMedida,
        decimal? CantidadDisponible,
        decimal? StockMinimo,
        DateOnly? FechaVencimiento,
        bool? Activo
    );
    
    public record RegistrarMovimientoDto(
        decimal Cantidad,
        string? Motivo,
        int UsuarioResponsableId,
        string? Observaciones,
        decimal? CostoUnitario
    );

}
