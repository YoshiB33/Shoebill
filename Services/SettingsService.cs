using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Shoebill.Models;

namespace Shoebill.Services;

public class SettingsService : ISettingsService
{
    private const string SettingsPath = "./settings.json";
    public Action<ApiKey, KeyUpdatedAction>? ApiKeyUpdated { get; set; }
    public SettingsService()
    {
        if (!new FileInfo(SettingsPath).Exists || new FileInfo(SettingsPath).Length <= 0)
        {
            WriteEmptySettings();
        }
    }
    public void WriteEmptySettings()
    {
        var emptyjson = JsonSerializer.Serialize(new SettingsModel {
            ApiKeys = []
        });
        File.WriteAllText(SettingsPath, emptyjson);
    }

    public async Task WriteEmptySettingsAsync()
    {
        using var fs = File.Open(SettingsPath, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(fs, new SettingsModel
        {
            ApiKeys = []
        });
    }

    public List<ApiKey>? GetAllApiKeys()
    {
        return JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath))?.ApiKeys;
    }

    public async Task<List<ApiKey>> GetAllApiKeysAsync()
    {
        using var fs = File.Open(SettingsPath, FileMode.OpenOrCreate);
        var settings = await JsonSerializer.DeserializeAsync<SettingsModel>(fs);
#pragma warning disable CS8603 // Possible null reference return.
        return settings?.ApiKeys;
#pragma warning restore CS8603 // Possible null reference return.
    }

    public void WriteApiKey(ApiKey apiKey)
    {
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath)); 
        currentSettings?.ApiKeys?.Add(apiKey);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(currentSettings));
        ApiKeyUpdated?.Invoke(apiKey, KeyUpdatedAction.Added);
    }

    public async Task WriteApiKeyAsync(ApiKey apiKey)
    {
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath));
        using var fs = File.Open(SettingsPath, FileMode.Truncate);
        currentSettings?.ApiKeys?.Add(apiKey);
        await JsonSerializer.SerializeAsync(fs, currentSettings);
        ApiKeyUpdated?.Invoke(apiKey, KeyUpdatedAction.Added);
    }

    public void RemoveApiKey(ApiKey apiKey)
    {
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath)); 
        currentSettings?.ApiKeys?.Remove(apiKey);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(currentSettings));
        ApiKeyUpdated?.Invoke(apiKey, KeyUpdatedAction.Removed);
    }

    public async Task RemoveApiAsync(ApiKey apiKey)
    {
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath));
        using var fs = File.Open(SettingsPath, FileMode.Truncate);
        currentSettings?.ApiKeys?.RemoveAll(x => x.Key == apiKey.Key);
        await JsonSerializer.SerializeAsync(fs, currentSettings);
        ApiKeyUpdated?.Invoke(apiKey, KeyUpdatedAction.Removed);
    }
}
