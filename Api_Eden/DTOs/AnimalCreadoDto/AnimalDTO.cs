using System.ComponentModel.DataAnnotations;

namespace Api_Eden.DTOs.AnimalCreadoDto
{
    public record AnimalDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Especie { get; set; }      
        public int? EspecieId { get; set; }
        public string? Raza { get; set; }
        public int? Edad { get; set; }
        public string? UnidadEdad { get; set; } = "años";
        public string? FechaNacimiento { get; set; }     
        public bool? FechaNacimientoEstimada { get; set; }
        public string? FechaIngreso { get; set; }  
        public string? Sexo { get; set; }
        public string? ZonaActual { get; set; }   
        public int? ZonaActualId { get; set; }
        public string? Color { get; set; }
        public string? FotografiaUrl { get; set; }
        public string? Observaciones { get; set; }
        public string? EstadoSalud { get; set; }
        public string? EstadoGeneral { get; set; }
    }
    public record ActualizarEstadoAnimalDto(
      string? EstadoGeneral,
      string? EstadoSalud
  );
}

