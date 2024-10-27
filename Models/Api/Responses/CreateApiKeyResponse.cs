using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public abstract record CreateApiKeyResponse(string Object, API_Key Attributes, CreateApiKeyResponseMeta Meta);

public abstract record CreateApiKeyResponseMeta(string Secret_token);