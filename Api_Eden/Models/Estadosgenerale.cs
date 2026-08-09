using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Estadosgenerale
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public string? EstadoAnterior { get; set; }

    public string EstadoNuevo { get; set; } = null!;

    public DateTime? FechaCambio { get; set; }

    public string? Motivo { get; set; }

    public string? Observaciones { get; set; }

    public int UsuarioResponsableId { get; set; }

    public string? LugarTransferencia { get; set; }

    public string? CausaFallecimiento { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Usuario UsuarioResponsable { get; set; } = null!;
}
