using System;

namespace Shoebill.Models.Api.Responses;

public record GetSSHResponse(string Object, GetSSHResponse_Data Data);

public record GetSSHResponse_Data(string Object, GetSSHResponse_Attributes Attributes);

public record GetSSHResponse_Attributes(string Name, string Fingerprint, string Public_key, DateTime Created_at);
