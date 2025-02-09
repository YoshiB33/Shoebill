namespace Shoebill.Models.Api.Requests;

public record UpdatePasswordRequest(string Current_password, string Password, string Password_confirmation);