using System;

namespace Shoebill.Models.Api.Schemas;

public record File_object(
    string Name,
    string Mode,
    string Mode_bits,
    int Size,
    bool Is_file,
    bool Is_symlink,
    string Mimetype,
    DateTime Created_at,
    DateTime Modified_at
);