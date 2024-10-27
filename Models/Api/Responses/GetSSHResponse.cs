using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public abstract record GetSshResponse(string Object, GetSshResponseData[] Data);

public abstract record GetSshResponseData(string Object, SSH_Key Attributes);