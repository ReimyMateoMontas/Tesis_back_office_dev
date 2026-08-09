using Api_Eden.DTOs.GastosDto;

namespace Api_Eden.Services.GastosService.Interface
{
    public interface IGastoService
    {
        Task<object> GetGastos();
        Task<(bool ok, string mensaje, object? data)> GetGasto(int id);
        Task<object> GetCategorias();
        Task<(bool ok, string mensaje)> CrearGasto(CrearGastoDto dto);
        Task<(bool ok, string mensaje)> ActualizarGasto(int id, ActualizarGastoDto dto);
        Task<(bool ok, string mensaje)> EliminarGasto(int id);
        Task<object> GetResumenMensual();
        Task<object> GetGastosPorCategoria(int year, int mes);
        Task<object> GetSerieMensual(int? year);
    }
}
