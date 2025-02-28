namespace Shoebill.Models.Api.Schemas;

public record Allocation(
    int Id,
    string Ip,
    string? Ip_alias,
    int Port,
    string? Notes,
    bool Is_default
);