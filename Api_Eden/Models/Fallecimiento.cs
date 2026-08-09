using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Fallecimiento
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? Hora { get; set; }

    public string Causa { get; set; } = null!;

    public string? Lugar { get; set; }

    public int? VeterinarioId { get; set; }

    public string? Observaciones { get; set; }

    public string? DocumentoAdjunto { get; set; }

    public int UsuarioRegistroId { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Usuario UsuarioRegistro { get; set; } = null!;

    public virtual Usuario? Veterinario { get; set; }
}
