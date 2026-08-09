namespace Api_Eden.Services.DashboardService
{
    public record DashboardStatsDto(
        int TotalAnimales,
        int Saludables,
        int EnTratamiento,
        int Criticos,
        int Recuperados,
        int AdoptadosMes,
        int IngresadosSemana,
        int AdopcionesPendientes,
        int TratamientosActivos,
        int AlertasStock
    );

    public record EstadoSaludDto(string Estado, int Cantidad);

    public record ZonaOcupacionDto(string Nombre, int Capacidad, int Ocupacion);

    public record GastoMensualDto(int Anio, int Mes, decimal Total);

    public record GastoCategoriaDto(string Categoria, decimal Total);

    public record ActividadDto(string Tipo, string Titulo, string Detalle, DateTime? Fecha);

    public record DashboardResumenDto(
        DashboardStatsDto Stats,
        IEnumerable<EstadoSaludDto> EstadoSalud,
        IEnumerable<ZonaOcupacionDto> Zonas,
        IEnumerable<GastoMensualDto> GastosMensuales,
        IEnumerable<GastoCategoriaDto> GastosPorCategoria,
        IEnumerable<ActividadDto> ActividadReciente
    );
}