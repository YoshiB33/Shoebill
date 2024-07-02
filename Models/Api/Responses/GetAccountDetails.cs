namespace Shoebill.Models.Api.Responses;

public record GetAccountDetails(string Object, GetAccountAttributes Attributes);

public record GetAccountAttributes(int Id, bool Admin, string Username, string Eamil, string First_Name, string Last_Name, string Language);
