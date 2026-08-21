namespace Api_Eden.DTOs.MedicoDto
{

    // ── Historial Médico ──────────────────────────────────────────────
    public record RegistrarHistorialDto(
        int AnimalId,
        string Diagnostico,
        string? Sintomas,
        decimal? Peso,
        decimal? Temperatura,
        int VeterinarioId,
        string? Observaciones
    );

    // ── Tratamiento ───────────────────────────────────────────────────
    public class RegistrarTratamientoDto
    {
        public int HistorialMedicoId { get; set; }
        public int MedicamentoId { get; set; }
        public string Dosis { get; set; } = null!;
        public string Frecuencia { get; set; } = null!;
        public string ViaAdministracion { get; set; } = null!;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int VeterinarioId { get; set; }
        public string? Observaciones { get; set; }
    }

    public record ActualizarEstadoTratamientoDto(
        string Estado
    );
    // ── Medicamento ───────────────────────────────────────────────────
    public record CrearMedicamentoDto(
        string Nombre,
        string? PrincipioActivo,
        string? Presentacion,
        string? Concentracion,
        string? Fabricante,
        string? Indicaciones,
        string? Contraindicaciones,
        string? EfectosSecundarios,
        bool RequiereReceta = false
    );

    // ── Vacuna ────────────────────────────────────────────────────────
    public record RegistrarVacunaDto(
        int AnimalId,
        int TipoVacunaId,
        DateOnly FechaAplicacion,
        DateOnly? ProximaDosis,
        string? Lote,
        int VeterinarioId,
        string? Observaciones
    );

    public record ActualizarEstadoVacunaDto(
        string Estado
    );

    // ── Fallecimiento ─────────────────────────────────────────────────
    public record RegistrarFallecimientoDto(
        int AnimalId,
        DateOnly FechaFallecimiento,
        string CausaFallecimiento,
        int VeterinarioId,
        int UsuarioRegistroId,
        string? Observaciones
    );
}
