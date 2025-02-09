using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record CreateSshKeyResponse(string Object, SSH_Key Attributes);