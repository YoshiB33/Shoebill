namespace Shoebill.Models.Api.Responses;

public record StatsWsResponse(
    long Memory_bytes,
    long Memory_limit_bytes,
    float Cpu_absolute,
    StatsWsResponseNetwork Network,
    int Uptime,
    string State,
    long Disk_bytes);

public record StatsWsResponseNetwork(int Rx_bytes, int Tx_bytes);