using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Shoebill.Models;

namespace Shoebill.Services;

public interface ISettingsService
{
    public Action<ApiKey, KeyUpdatedAction>? ApiKeyUpdated { get; set; }
    public void WriteEmptySettings();
    public Task WriteEmptySettingsAsync();
    public List<ApiKey>? GetAllApiKeys();
    public Task<List<ApiKey>>? GetAllApiKeysAsync();
    public void WriteApiKey(ApiKey apiKey);
    public Task WriteApiKeyAsync(ApiKey apiKey);
    public void RemoveApiKey(ApiKey apiKey);
    public Task RemoveApiAsync(ApiKey apiKey);
}
