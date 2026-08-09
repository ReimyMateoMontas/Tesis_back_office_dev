using System.ComponentModel.DataAnnotations;

namespace Api_Eden.DTOs.Zone.Request
{
    public class MoverAnimalDto
    {
        [Required(ErrorMessage = "El ID del animal es obligatorio")]
        public int AnimalId { get; set; }

        [Required(ErrorMessage = "La zona destino es obligatoria")]
        public int ZonaDestinoId { get; set; }

        [StringLength(300)]
        public string? Motivo { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }
    }
}