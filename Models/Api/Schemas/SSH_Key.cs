using System;

namespace Shoebill.Models.Api.Schemas;

public record SSH_Key(string Name, string Fingerprint, string Public_key, DateTime Created_at);

