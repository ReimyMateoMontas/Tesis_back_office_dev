using System.ComponentModel.DataAnnotations;

namespace Api_Eden.DTOs.AnimalCreadoDto
{
    public class CrearAnimalDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "La especie es obligatoria")]
        public int EspecieId { get; set; }

        [StringLength(100)]
        public string? Raza { get; set; }

        [Range(0, 999, ErrorMessage = "La edad debe estar entre 0 y 999")]
        public int? Edad { get; set; }

        public string UnidadEdad { get; set; } = "años";

        public DateOnly? FechaNacimiento { get; set; }

        public bool? FechaNacimientoEstimada { get; set; }

        public DateOnly? FechaIngreso { get; set; }

        [StringLength(50)]
        public string? Sexo { get; set; }

        public int? ZonaActualId { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        public string? FotografiaUrl { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        public int? UsuarioRegistroId { get; set; }
    }
}