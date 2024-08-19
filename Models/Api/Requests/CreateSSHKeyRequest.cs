namespace Shoebill.Models.Api.Requests;

public record CreateSSHKeyRequest(string Name, string Public_key);
