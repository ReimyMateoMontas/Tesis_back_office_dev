using Api_Eden.DTOs.MedicoDto;

namespace Api_Eden.Services.TratamientoService.Interface
{
    public interface IVacunaService
    {
        Task<(bool ok, string mensaje, int? id)> RegistrarVacuna(RegistrarVacunaDto dto);
        Task<(bool ok, string mensaje, object? data)> GetVacunasPorAnimal(int animalId);
    }
}
