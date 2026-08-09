using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Animale
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int EspecieId { get; set; }

    public string? Raza { get; set; }

    public int? Edad { get; set; }

    public string UnidadEdad { get; set; } = null!;

    public DateOnly? FechaNacimiento { get; set; }

    public bool? FechaNacimientoEstimada { get; set; }

    public string? Sexo { get; set; }

    public string? Color { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public int? ZonaActualId { get; set; }

    public string? EstadoSalud { get; set; }

    public string? EstadoGeneral { get; set; }

    public string? FotografiaUrl { get; set; }

    public string? Observaciones { get; set; }

    public int? UsuarioRegistroId { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaUltimaModificacion { get; set; }

    public DateOnly? FechaFallecimiento { get; set; }

    public DateOnly? FechaAdopcion { get; set; }

    public DateOnly? FechaTransferencia { get; set; }

    public string? LugarTransferencia { get; set; }

    public virtual ICollection<Adopcione> Adopciones { get; set; } = new List<Adopcione>();

    public virtual Especy Especie { get; set; } = null!;

    public virtual ICollection<Estadosgenerale> Estadosgenerales { get; set; } = new List<Estadosgenerale>();

    public virtual ICollection<Fallecimiento> Fallecimientos { get; set; } = new List<Fallecimiento>();

    public virtual ICollection<Historialmedico> Historialmedicos { get; set; } = new List<Historialmedico>();

    public virtual ICollection<Historialmovimiento> Historialmovimientos { get; set; } = new List<Historialmovimiento>();

    public virtual ICollection<Transferencia> Transferencia { get; set; } = new List<Transferencia>();

    public virtual Usuario? UsuarioRegistro { get; set; }

    public virtual ICollection<Vacuna> Vacunas { get; set; } = new List<Vacuna>();

    public virtual Zona? ZonaActual { get; set; }
}
