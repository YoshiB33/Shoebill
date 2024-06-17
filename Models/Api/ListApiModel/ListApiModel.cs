using System;
using System.Collections.Generic;

namespace Shoebill.Models.Api.ListApiModel;

public record Allocations(string @object, IReadOnlyList<Datum> data);

public record Attributes(
   bool server_owner,
   string identifier,
   int internal_id,
   string uuid,
   string name,
   string node,
   bool is_node_under_maintenance,
   SftpDetails sftp_details,
   string description,
   Limits limits,
   string invocation,
   string docker_image,
   IReadOnlyList<string> egg_features,
   FeatureLimits feature_limits,
   string? status,
   bool is_suspended,
   bool is_installing,
   bool is_transferring,
   Relationships relationships,
   int id,
   string ip,
   string ip_alias,
   int port,
   object notes,
   bool is_default,
   string env_variable,
   string default_value,
   string server_value,
   bool is_editable,
   string rules
);

public record Datum(string @object, Attributes attributes);

public record FeatureLimits(int databases, int allocations, int backups);

public record Limits(
    int memory,
    int swap,
    int disk,
    int io,
    int cpu,
    string threads,
    bool oom_disabled
);

public record Links();

public record Meta(Pagination pagination);

public record Pagination(
    int total,
    int count,
    int per_page,
    int current_page,
    int total_pages,
    Links links
);

public record Relationships(Allocations allocations, Variables variables);

public record ListApiModel(string @object, IReadOnlyList<Datum> data, Meta meta);

public record SftpDetails(string ip, int port);

public record Variables(string @object, IReadOnlyList<Datum> data);
