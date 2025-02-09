using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public record GetApiKeys(string Object, List<GetApiKeysData> Data);

public record GetApiKeysData(string Object, API_Key Attributes);