using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetServerDetailsResponse(string Object, Server Attributes, Meta Meta);

public record Meta(bool IsServerOwner, string[] UserPermissions);