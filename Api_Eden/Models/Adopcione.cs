using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Adopcione
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public string NombreAdoptante { get; set; } = null!;

    public string? TelefonoAdoptante { get; set; }

    public string? EmailAdoptante { get; set; }

    public string? DireccionAdoptante { get; set; }

    public string? DocumentoIdentidad { get; set; }

    public DateOnly FechaAdopcion { get; set; }

    public DateOnly? FechaSeguimiento { get; set; }

    public string? EstadoAdopcion { get; set; }

    public int UsuarioResponsableId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Animale Animal { get; set; } = null!;

    public virtual Usuario UsuarioResponsable { get; set; } = null!;
}
