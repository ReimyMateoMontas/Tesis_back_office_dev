using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Rol { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaUltimaModificacion { get; set; }

    public bool EmailVerificado { get; set; }

    public string? TokenActivacion { get; set; }

    public DateTime? TokenExpiracion { get; set; }

    public string? FotoPerfilUrl { get; set; }

    public virtual ICollection<Adopcione> Adopciones { get; set; } = new List<Adopcione>();

    public virtual ICollection<Animale> Animales { get; set; } = new List<Animale>();

    public virtual ICollection<Cierresmensuale> Cierresmensuales { get; set; } = new List<Cierresmensuale>();

    public virtual ICollection<Donacione> Donaciones { get; set; } = new List<Donacione>();

    public virtual ICollection<Estadosgenerale> Estadosgenerales { get; set; } = new List<Estadosgenerale>();

    public virtual ICollection<Fallecimiento> FallecimientoUsuarioRegistros { get; set; } = new List<Fallecimiento>();

    public virtual ICollection<Fallecimiento> FallecimientoVeterinarios { get; set; } = new List<Fallecimiento>();

    public virtual ICollection<Gasto> Gastos { get; set; } = new List<Gasto>();

    public virtual ICollection<Historialmedico> Historialmedicos { get; set; } = new List<Historialmedico>();

    public virtual ICollection<Historialmovimiento> Historialmovimientos { get; set; } = new List<Historialmovimiento>();

    public virtual ICollection<Movimientosinventario> Movimientosinventarios { get; set; } = new List<Movimientosinventario>();

    public virtual ICollection<Objetivo> Objetivos { get; set; } = new List<Objetivo>();

    public virtual ICollection<Transferencia> Transferencia { get; set; } = new List<Transferencia>();

    public virtual ICollection<Tratamiento> Tratamientos { get; set; } = new List<Tratamiento>();

    public virtual ICollection<Vacuna> Vacunas { get; set; } = new List<Vacuna>();
}
