using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Especy
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string? CuidadosEspeciales { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Animale> Animales { get; set; } = new List<Animale>();

    public virtual ICollection<Tiposvacuna> Tiposvacunas { get; set; } = new List<Tiposvacuna>();
}
