namespace Shoebill.Models.Api.Schemas;

public record Error(string Code, string Status, string Detail, Error_meta Meta);

public record Error_meta(string Source_field, string Rule);
