using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Cierresmensuale
{
    public int Id { get; set; }

    public int Año { get; set; }

    public int Mes { get; set; }

    public decimal TotalIngresos { get; set; }

    public decimal TotalEgresos { get; set; }

    public decimal Balance { get; set; }

    public decimal? BalanceAnterior { get; set; }

    public decimal? BalanceFinal { get; set; }

    public DateTime FechaCierre { get; set; }

    public int UsuarioCierreId { get; set; }

    public string? Observaciones { get; set; }

    public virtual Usuario UsuarioCierre { get; set; } = null!;
}
