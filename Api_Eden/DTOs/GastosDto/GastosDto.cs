namespace Api_Eden.DTOs.GastosDto
{
    public record CrearGastoDto(
     int CategoriaGastoId,
     string Concepto,
     decimal Monto,
     DateOnly FechaGasto,
     string FormaPago,        // Efectivo, Transferencia, Tarjeta, Cheque
     string? NumeroFactura,
     string? NumeroTransaccion,
     string? NombreProveedor,
     string? TelefonoProveedor,
     int? AlimentoId,
     int? MedicamentoId,
     string? Observaciones,
     int UsuarioRegistroId
 );

    public record ActualizarGastoDto(
        string? Concepto,
        decimal? Monto,
        string? FormaPago,
        string? NumeroFactura,
        string? NumeroTransaccion,
        string? NombreProveedor,
        string? TelefonoProveedor,
        string? Observaciones
    );
}
