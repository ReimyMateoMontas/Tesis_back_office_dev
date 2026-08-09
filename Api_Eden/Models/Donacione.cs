using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Donacione
{
    public int Id { get; set; }

    public int? DonanteId { get; set; }

    public int TipoDonacionId { get; set; }

    public decimal? MontoDinero { get; set; }

    public string? DescripcionDonacion { get; set; }

    public int? CantidadArticulos { get; set; }

    public decimal? ValorEstimado { get; set; }

    public int? AlimentoId { get; set; }

    public int? MedicamentoId { get; set; }

    public DateOnly FechaDonacion { get; set; }

    public string? FormaPago { get; set; }

    public string? NumeroTransaccion { get; set; }

    public bool? RequiereCertificado { get; set; }

    public bool? CertificadoGenerado { get; set; }

    public DateOnly? FechaCertificado { get; set; }

    public string? DocumentoAdjunto { get; set; }

    public int UsuarioRegistroId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public int? ObjetivoId { get; set; }

    public virtual Alimento? Alimento { get; set; }

    public virtual Donante? Donante { get; set; }

    public virtual Medicamento? Medicamento { get; set; }

    public virtual Objetivo? Objetivo { get; set; }

    public virtual Tiposdonacion TipoDonacion { get; set; } = null!;

    public virtual Usuario UsuarioRegistro { get; set; } = null!;
}
