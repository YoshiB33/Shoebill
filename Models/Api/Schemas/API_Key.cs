using System;
using System.Collections.Generic;

namespace Shoebill.Models.Api.Schemas;

public record API_Key(
    string Identifier,
    string Description,
    List<string> Allowed_ips,
    DateTime? Last_used_at,
    DateTime Created_at
);