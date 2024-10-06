using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetSSHResponse(string Object, GetSSHResponse_Data[] Data);

public record GetSSHResponse_Data(string Object, SSH_Key Attributes);

