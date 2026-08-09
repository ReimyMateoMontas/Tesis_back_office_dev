using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Categoriasgasto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual ICollection<Presupuestomensual> Presupuestomensuals { get; set; } = new List<Presupuestomensual>();
}
