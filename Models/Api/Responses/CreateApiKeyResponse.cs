using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record CreateApiKeyResponse(string Object, API_Key Attributes, CreateApiKeyResponse_Meta Meta);

public record CreateApiKeyResponse_Meta(string Secret_token);
