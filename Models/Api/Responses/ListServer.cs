using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record class ListServer(
    string Object,
    List<ListServer_Data> Data,
    ListServer_Meta Meta
);

public record ListServer_Data(
    string Object,
    Server Attributes
);

public record ListServer_Meta(
    Pagination Pagination
);