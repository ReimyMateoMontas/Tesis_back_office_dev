using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Historialmedico
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public DateTime? Fecha { get; set; }

    public string Diagnostico { get; set; } = null!;

    public string? Sintomas { get; set; }

    public decimal? Peso { get; set; }

    public decimal? Temperatura { get; set; }

    public int VeterinarioId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    public virtual Usuario Veterinario { get; set; } = null!;
}
