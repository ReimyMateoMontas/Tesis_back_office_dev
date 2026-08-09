using System;
using System.Collections.Generic;

namespace Api_Eden.Models;

public partial class Gasto
{
    public int Id { get; set; }

    public int CategoriaGastoId { get; set; }

    public string Concepto { get; set; } = null!;

    public decimal Monto { get; set; }

    public DateOnly FechaGasto { get; set; }

    public string FormaPago { get; set; } = null!;

    public string? NumeroFactura { get; set; }

    public string? NumeroTransaccion { get; set; }

    public string? NombreProveedor { get; set; }

    public string? TelefonoProveedor { get; set; }

    public int? AlimentoId { get; set; }

    public int? MedicamentoId { get; set; }

    public string? DocumentoAdjunto { get; set; }

    public int UsuarioRegistroId { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual Alimento? Alimento { get; set; }

    public virtual Categoriasgasto CategoriaGasto { get; set; } = null!;

    public virtual Medicamento? Medicamento { get; set; }

    public virtual Usuario UsuarioRegistro { get; set; } = null!;
}
