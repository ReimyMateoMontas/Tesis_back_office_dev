using Api_Eden.DTOs.MedicoDto;

namespace Api_Eden.Services.TratamientoService.Interface
{
    public interface IFallecimientoService
    {
        Task<(bool ok, string mensaje)> RegistrarFallecimiento(RegistrarFallecimientoDto dto);
    }
}
