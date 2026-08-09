using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Objetivo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal MontoObjetivo { get; set; }

    public decimal MontoRecaudado { get; set; }

    public string Estado { get; set; } = null!;

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaLimite { get; set; }

    public int UsuarioCreoId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();

    public virtual Usuario UsuarioCreo { get; set; } = null!;
}
