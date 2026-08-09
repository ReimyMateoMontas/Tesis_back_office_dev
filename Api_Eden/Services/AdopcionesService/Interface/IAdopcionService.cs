using Api_Eden.DTOs.AdopcionDto;

namespace Api_Eden.Services.AdopcionesService.Interface
{

    public interface IAdopcionService
    {
        Task<object> GetAdopciones();
        Task<(bool ok, string mensaje, object? data)> GetAdopcion(int id);
        Task<(bool ok, string mensaje)> RegistrarAdopcion(RegistrarAdopcionDto dto);
        Task<(bool ok, string mensaje)> ActualizarEstado(int id, ActualizarEstadoAdopcionDto dto);
    }
}
