using Api_Eden.DTOs.MedicoDto;

namespace Api_Eden.Services.TratamientoService.Interface
{

    public interface ITratamientoService
    {
        Task<(bool ok, string mensaje, int? id)> RegistrarTratamiento(RegistrarTratamientoDto dto);
        Task<(bool ok, string mensaje)> ActualizarEstadoTratamiento(int id, string estado, int veterinarioId);
    }
}
