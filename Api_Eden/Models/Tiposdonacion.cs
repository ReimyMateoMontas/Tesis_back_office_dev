using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Tiposdonacion
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();
}
