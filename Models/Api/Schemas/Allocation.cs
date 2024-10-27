namespace Shoebill.Models.Api.Schemas;

public abstract record Allocation(
    int Number,
    string Ip,
    string? Ip_alias,
    int Port,
    string? Notes,
    bool Is_default
);