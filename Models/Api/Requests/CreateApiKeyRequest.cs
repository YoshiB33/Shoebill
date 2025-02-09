using System.Collections;

namespace Shoebill.Models.Api.Requests;

public record CreateApiKeyRequest(string Description, IEnumerable Allowed_ips);