using Api_Eden.DTOs.DonacionesDto;

namespace Api_Eden.Services.DonacionesService.Interface
{
    public interface IDonacionService
    {
        Task<IEnumerable<DonacionResponseDto>> GetAllAsync();
        Task<ResumenDonacionesDto> GetResumenAsync();
        Task<(bool ok, string mensaje, int? id)> RegistrarAsync(RegistrarDonacionDto dto, int usuarioId);
        Task<(bool ok, string mensaje)> EliminarAsync(int id);
        Task<IEnumerable<object>> GetTiposAsync();
        Task<IEnumerable<object>> GetDonantesAsync();
    }
}
