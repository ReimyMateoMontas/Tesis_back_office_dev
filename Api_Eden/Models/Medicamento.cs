using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Medicamento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? PrincipioActivo { get; set; }

    public string? Presentacion { get; set; }

    public string? Concentracion { get; set; }

    public string? Fabricante { get; set; }

    public string? Indicaciones { get; set; }

    public string? Contraindicaciones { get; set; }

    public string? EfectosSecundarios { get; set; }

    public bool? RequiereReceta { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();
}
