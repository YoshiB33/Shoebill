using System.Collections.Generic;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Models.Api.Responses;

public abstract record GetApiKeys(string Object, List<GetApiKeysData> Data);

public abstract record GetApiKeysData(string Object, API_Key Attributes);