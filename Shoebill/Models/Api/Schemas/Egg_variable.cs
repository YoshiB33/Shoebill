namespace Shoebill.Models.Api.Schemas;

public record Egg_variable(
    string Name,
    string Description,
    string Env_variable,
    string Default_value,
    string Server_value,
    bool IsEditable,
    string Rules
);