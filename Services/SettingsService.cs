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

    public SettingsService()
    {
        if (!new FileInfo(SettingsPath).Exists || new FileInfo(SettingsPath).Length <= 0)
        {
            WriteEmptySettings();
        }
    }

    public Action<ApiKey, KeyUpdatedAction>? ApiKeyUpdated { get; set; }

    public void WriteEmptySettings()
    {
        var emptyJson = JsonSerializer.Serialize(new SettingsModel
        {
            ApiKeys = []
        });
        File.WriteAllText(SettingsPath, emptyJson);
    }

    public async Task WriteEmptySettingsAsync()
    {
        await using var fs = File.Open(SettingsPath, FileMode.OpenOrCreate);
        await JsonSerializer.SerializeAsync(fs, new SettingsModel
        {
            ApiKeys = []
        });
    }

    public List<ApiKey>? GetAllApiKeys()
    {
        return JsonSerializer.Deserialize<SettingsModel>(File.ReadAllText(SettingsPath))?.ApiKeys;
    }

    public async Task<List<ApiKey>?> GetAllApiKeysAsync()
    {
        await using var fs = File.Open(SettingsPath, FileMode.OpenOrCreate);
        var settings = await JsonSerializer.DeserializeAsync<SettingsModel>(fs);
        return settings?.ApiKeys;
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
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(await File.ReadAllTextAsync(SettingsPath));
        await using (var fs = File.Open(SettingsPath, FileMode.Truncate))
        {
            currentSettings?.ApiKeys?.Add(apiKey);
            await JsonSerializer.SerializeAsync(fs, currentSettings);
        }

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
        var currentSettings = JsonSerializer.Deserialize<SettingsModel>(await File.ReadAllTextAsync(SettingsPath));
        await using (var fs = File.Open(SettingsPath, FileMode.Truncate))
        {
            currentSettings?.ApiKeys?.RemoveAll(x => x.Key == apiKey.Key);
            await JsonSerializer.SerializeAsync(fs, currentSettings);
        }

        ApiKeyUpdated?.Invoke(apiKey, KeyUpdatedAction.Removed);
    }
}