

using Api_Eden.DTOs.ObjectivoDto;


namespace Api_Eden.Services.ObjetivoService.Interface
{
    public interface IObjetivoService
    {
        Task<IEnumerable<ObjetivoResponseDto>> GetAllAsync();
        Task<(bool ok, string mensaje, int? id)> CrearAsync(CrearObjetivoDto dto, int usuarioId);
        Task<(bool ok, string mensaje)> ActualizarAsync(int id, ActualizarObjetivoDto dto);
        Task<(bool ok, string mensaje)> EliminarAsync(int id);
    }
}