using System.Collections.Generic;

namespace Shoebill.Models.Api.Schemas;

public record Server(
    bool Server_owner,
    string Identifier,
    int Internal_id,
    string Uuid,
    string Name,
    string Node,
    SFPT_details SFTP_Details,
    string Description,
    Server_Limits Limits,
    string Invocation,
    string Docker_image,
    List<string> Egg_features,
    Server_Feature_limits Feature_Limits,
    string? Status,
    bool Is_suspended,
    bool Is_installing,
    bool Is_transfering,
    Server_relationships Relationships
);

public record Server_Limits(
    int Memory,
    int Swap,
    int Disk,
    int Io,
    int Cpu,
    string? Threads,
    bool Oom_disabled
);

public record Server_Feature_limits(int Databases, int Allocations, int Backups);

public record Server_relationships(
    Allocations Allocations,
    Variables Variables,
    Egg? Egg,
    SubUsers? SubUsers
);

public record Allocations(string Object, List<Allocation_Data> Data);

public record Allocation_Data(string Object, Allocation Attributes);

public record Variables(string Object, List<Variable_data> Data);

public record Variable_data(string Object, Egg_variable Attributes);

public record Egg(string Object, Egg_attributes Attributes);

public record Egg_attributes(string Uuid, string Name);

public record SubUsers(string Object, List<object> Data);