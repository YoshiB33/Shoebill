using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetApiKeys(string Object, List<GetApiKeys_Data> Data);

public record GetApiKeys_Data(string Object, API_Key Attributes);
