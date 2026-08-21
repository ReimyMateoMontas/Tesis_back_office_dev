using Api_Eden.DTOs.MedicoDto;

namespace Api_Eden.Services.TratamientoService.Interface
{
    public interface IVacunaService
    {
        Task<(bool ok, string mensaje, int? id)> RegistrarVacuna(RegistrarVacunaDto dto);
        Task<(bool ok, string mensaje, object? data)> GetVacunasPorAnimal(int animalId);

        // Marca una vacuna como 'Pendiente' o 'Completada' (igual que un tratamiento).
        Task<(bool ok, string mensaje)> ActualizarEstadoVacuna(int id, string estado, int usuarioId);

        // Devuelve las vacunas cuya próxima dosis está vencida o es hoy y siguen 'Pendiente'.
        Task<(bool ok, string mensaje, object? data)> GetAlertasVacunas();
    }
}
