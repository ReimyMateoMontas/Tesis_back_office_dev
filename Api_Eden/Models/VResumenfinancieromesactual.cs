using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class VResumenfinancieromesactual
{
    public string Periodo { get; set; } = null!;

    public decimal? TotalIngresos { get; set; }

    public decimal? TotalEgresos { get; set; }

    public decimal? Balance { get; set; }
}
