using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public abstract record GetServerDetails(string Object, Server Attributes, Meta Meta);

public abstract record Meta(bool IsServerOwner, string[] User_permissions);