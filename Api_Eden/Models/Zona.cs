using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Zona
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public int CapacidadMaxima { get; set; }

    public int? CantidadActual { get; set; }

    public bool? Activa { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Animale> Animales { get; set; } = new List<Animale>();

    public virtual ICollection<Historialmovimiento> HistorialmovimientoZonaDestinos { get; set; } = new List<Historialmovimiento>();

    public virtual ICollection<Historialmovimiento> HistorialmovimientoZonaOrigens { get; set; } = new List<Historialmovimiento>();
}
