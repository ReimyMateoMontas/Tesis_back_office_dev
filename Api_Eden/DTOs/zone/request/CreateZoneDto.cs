using System.ComponentModel.DataAnnotations;

namespace Api_Eden.DTOs.Zone.Request
{
    public class CreateZoneDto
    {
        [Required]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "El nombre debe tener entre 4 y 100 caracteres")]
        public required string Nombre { get; set; }

        [StringLength(500)]
        public string? Descripcion { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "La capacidad máxima debe ser un número positivo")]
        [Required]
        public int CapacidadMaxima { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad actual debe ser un número positivo o cero")]
        [Required]
        public int CantidadActual { get; set; }

        [Required]
        public bool Activa { get; set; }

        // Add additional properties as needed, e.g., coordinates, area, etc.
    }
}