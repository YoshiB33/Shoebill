using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetSshResponse(string Object, GetSshResponseData[] Data);

public record GetSshResponseData(string Object, SSH_Key Attributes);