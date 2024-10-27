using System.Collections.Generic;

namespace Shoebill.Models.Api.Schemas;

public record Backup(string Object, Backup_attributes Attributes);

public abstract record Backup_attributes(
    string Uuid,
    string Name,
    List<string> Ignored_files,
    string? Checksum,
    int Bytes,
    bool Is_locked,
    bool Is_successful,
    string Created_at,
    string Completed_at
);