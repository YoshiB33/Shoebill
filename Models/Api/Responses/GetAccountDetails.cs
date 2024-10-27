namespace Shoebill.Models.Api.Responses;

public abstract record GetAccountDetails(string Object, GetAccountAttributes Attributes);

public abstract record GetAccountAttributes(
    int Id,
    bool Admin,
    string Username,
    string Email,
    string First_Name,
    string Last_Name,
    string Language);