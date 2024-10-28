using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record CreateApiKeyResponse(string Object, API_Key Attributes, CreateApiKeyResponseMeta Meta);

public record CreateApiKeyResponseMeta(string SecretToken);