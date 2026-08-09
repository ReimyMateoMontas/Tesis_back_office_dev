using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Donante
{
    public int Id { get; set; }

    public string TipoDonante { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string? DocumentoIdentidad { get; set; }

    public string? Rnc { get; set; }

    public string? Telefono { get; set; }

    public string? Email { get; set; }

    public string? Direccion { get; set; }

    public bool? EsRecurrente { get; set; }

    public string? FrecuenciaDonacion { get; set; }

    public DateOnly? FechaRegistro { get; set; }

    public string? Observaciones { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();
}
