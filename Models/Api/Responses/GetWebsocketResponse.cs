namespace Shoebill.Models.Api.Responses;

public record GetWebsocketResponse(GetWebsocketResponseData Data);

public abstract record GetWebsocketResponseData(string Token, string Socket);