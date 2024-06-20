using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetServerDetails(string Object, Server Attributes, Meta Meta);

public record Meta(bool IsServerOwner, string[] User_permissions);
