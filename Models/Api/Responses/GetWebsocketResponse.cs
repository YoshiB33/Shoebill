namespace Shoebill.Models.Api.Responses;

public record GetWebsocketResponse(GetWebsocketResponseData Data);

public record GetWebsocketResponseData(string Token, string Socket);