using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Transferencia
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public DateOnly FechaTransferencia { get; set; }

    public string LugarDestino { get; set; } = null!;

    public string? DireccionDestino { get; set; }

    public string? ContactoDestino { get; set; }

    public string? TelefonoDestino { get; set; }

    public string? EmailDestino { get; set; }

    public string MotivoTransferencia { get; set; } = null!;

    public string? DocumentoTransferencia { get; set; }

    public int UsuarioResponsableId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Usuario UsuarioResponsable { get; set; } = null!;
}
