using System.Collections.Generic;

namespace Shoebill.Models.Api.Schemas;

public abstract record Server(
    bool Server_owner,
    string Identifier,
    int Internal_id,
    string Uuid,
    string Name,
    string Node,
    SFPT_details SFPT_Details,
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

public abstract record Server_Limits(
    int Memory,
    int Swap,
    int Disk,
    int Io,
    int Cpu,
    string? Threads,
    bool Oom_disabled
);

public abstract record Server_Feature_limits(int Databases, int Allocations, int Backups);

public abstract record Server_relationships(
    Allocations Allocations,
    Variables Variables,
    Egg Egg,
    SubUsers SubUsers
);

public abstract record Allocations(string Object, List<Allocation_Data> Data);

public abstract record Allocation_Data(string Object, Allocation Attributes);

public abstract record Variables(string Object, List<Variable_data> Data);

public abstract record Variable_data(string Object, Egg_variable Attributes);

public abstract record Egg(string Object, Egg_attributes Attributes);

public abstract record Egg_attributes(string Uuid, string Name);

public abstract record SubUsers(string Object, List<object> Data);