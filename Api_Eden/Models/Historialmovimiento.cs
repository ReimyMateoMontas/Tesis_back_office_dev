using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Historialmovimiento
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public int? ZonaOrigenId { get; set; }

    public int? ZonaDestinoId { get; set; }

    public string? Motivo { get; set; }

    public DateTime? Fecha { get; set; }

    public int UsuarioResponsableId { get; set; }

    public string? Observaciones { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Usuario UsuarioResponsable { get; set; } = null!;

    public virtual Zona? ZonaDestino { get; set; }

    public virtual Zona? ZonaOrigen { get; set; }
}
