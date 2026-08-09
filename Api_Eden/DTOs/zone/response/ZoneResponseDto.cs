namespace Api_Eden.DTOs.Zone.Response;

public record ZoneResponseDto(
    int Id,
    string Name,
    string? Description,
    int MaxCapacity,
    int? CurrentCapacity,
    bool? IsActive,
    DateTime? CreatedAt
);