namespace Shoebill.Models.Api.Requests;

public record CreateSshKeyRequest(string Name, string Public_key);