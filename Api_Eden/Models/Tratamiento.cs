using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Tratamiento
{
    public int Id { get; set; }

    public int HistorialMedicoId { get; set; }

    public int MedicamentoId { get; set; }

    public string Dosis { get; set; } = null!;

    public string Frecuencia { get; set; } = null!;

    public string? ViaAdministracion { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public string? Estado { get; set; }

    public int VeterinarioId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Historialmedico HistorialMedico { get; set; } = null!;

    public virtual Medicamento Medicamento { get; set; } = null!;

    public virtual Usuario Veterinario { get; set; } = null!;
}
