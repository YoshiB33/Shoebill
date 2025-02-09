namespace Shoebill.Models.Api.Responses;

public record GetAccountDetails(string Object, GetAccountAttributes Attributes);

public record GetAccountAttributes(
    int Id,
    bool Admin,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string Language);