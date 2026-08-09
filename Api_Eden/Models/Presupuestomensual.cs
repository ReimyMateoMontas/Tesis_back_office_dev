using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Presupuestomensual
{
    public int Id { get; set; }

    public int Año { get; set; }

    public int Mes { get; set; }

    public int CategoriaGastoId { get; set; }

    public decimal MontoPresupuestado { get; set; }

    public decimal? MontoEjecutado { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Categoriasgasto CategoriaGasto { get; set; } = null!;
}
