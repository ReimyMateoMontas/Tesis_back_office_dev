using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Movimientosinventario
{
    public int Id { get; set; }

    public int AlimentoId { get; set; }

    public string TipoMovimiento { get; set; } = null!;

    public decimal Cantidad { get; set; }

    public string? Motivo { get; set; }

    public DateTime? FechaMovimiento { get; set; }

    public int UsuarioResponsableId { get; set; }

    public string? Observaciones { get; set; }

    public decimal? CostoUnitario { get; set; }

    public virtual Alimento Alimento { get; set; } = null!;

    public virtual Usuario UsuarioResponsable { get; set; } = null!;
}
