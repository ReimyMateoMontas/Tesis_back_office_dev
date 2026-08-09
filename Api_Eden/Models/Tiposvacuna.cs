using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Tiposvacuna
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int EspecieId { get; set; }

    public string? Descripcion { get; set; }

    public int? EdadMinima { get; set; }

    public int? DuracionMeses { get; set; }

    public bool? Obligatoria { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Especy Especie { get; set; } = null!;

    public virtual ICollection<Vacuna> Vacunas { get; set; } = new List<Vacuna>();
}
