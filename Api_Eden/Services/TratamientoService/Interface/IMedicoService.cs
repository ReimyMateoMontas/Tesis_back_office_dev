using Api_Eden.DTOs.MedicoDto;

namespace Api_Eden.Services.TratamientoService.Interface
{
    public interface IMedicoService
    {
        Task<IEnumerable<object>> GetHistorialAsync(int animalId);
        Task<(bool ok, string mensaje, int? id)> RegistrarHistorialAsync(RegistrarHistorialDto dto);
        Task<IEnumerable<object>> GetTratamientosAsync();
        Task<IEnumerable<object>> GetAllTratamientosAsync();
        Task<IEnumerable<object>> GetMedicamentosAsync();
        Task<(bool ok, object? data)> CrearMedicamentoAsync(string nombre);
        Task<IEnumerable<object>> GetTiposVacunaAsync();
        Task<IEnumerable<object>> GetVeterinariosAsync();

        // Historial completo por animal (consultas + vacunas + fallecimiento separados)
        Task<object?> GetHistorialCompletoAnimalAsync(int animalId);

        // Timeline unificada por animal: consultas, vacunas, fallecimiento en una
        // sola lista plana ordenada por fecha descendente, lista para renderizar
        Task<IEnumerable<object>> GetTimelineAnimalAsync(int animalId);
    }
}