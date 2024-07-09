namespace Shoebill.Models.Api.Requests;

public record UpadtePasswordRequest(string Current_password, string Password, string Password_confirmation);
