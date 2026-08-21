using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Vacuna
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public int TipoVacunaId { get; set; }

    public DateOnly FechaAplicacion { get; set; }

    public DateOnly? ProximaDosis { get; set; }

    public string? Lote { get; set; }

    public int VeterinarioId { get; set; }

    public string? Observaciones { get; set; }

    // Estado del ciclo de la vacuna: 'Pendiente' (próxima dosis por aplicar)
    // o 'Completada' (el usuario la marcó como finalizada / no requiere más dosis).
    public string? Estado { get; set; }

    // Fecha en la que se envió la última alerta por correo (evita reenvíos el mismo día).
    public DateOnly? AlertaEnviada { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Tiposvacuna TipoVacuna { get; set; } = null!;

    public virtual Usuario Veterinario { get; set; } = null!;
}
