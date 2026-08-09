using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Alimento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string TipoAnimal { get; set; } = null!;

    public string? Marca { get; set; }

    public string UnidadMedida { get; set; } = null!;

    public decimal CantidadDisponible { get; set; }

    public decimal StockMinimo { get; set; }

    public DateOnly? FechaVencimiento { get; set; }

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual ICollection<Movimientosinventario> Movimientosinventarios { get; set; } = new List<Movimientosinventario>();
}
