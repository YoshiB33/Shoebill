using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.Requests;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Services;

public class ApiService(
    HttpClient httpClient
) : IApiService
{
    private ApiKey? ApiKey { get; set; }
    public string? CurrentServerUuid { get; set; }
    public Server? CurrentServer { get; set; }
    public GetAccountAttributes? CurrentAccount { get; set; }
    private readonly JsonSerializerOptions jsonSettings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public void SetApiKey(ApiKey? apiKey)
    {
        ApiKey = apiKey;
        httpClient.BaseAddress = new Uri($"https://{apiKey?.ServerAdress}/api/");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey?.Key);
    }

    public async Task<T?> StandardGetAsync<T>(string Path)
    {
        using var response = await httpClient.GetAsync(Path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
    public async Task StandardPutAsync<T>(string Path, T Body)
    {
        var response = await httpClient.PutAsJsonAsync(Path, Body);
        response.EnsureSuccessStatusCode();
    }
    public async Task<T2?> StandardPostAsync<T1, T2>(string Path, T1 Body)
    {
        var response = await httpClient.PostAsJsonAsync(Path, Body);
        response.EnsureSuccessStatusCode();
        var deJson = await JsonSerializer.DeserializeAsync<T2>(await response.Content.ReadAsStreamAsync());
        return deJson;
    }
    public async Task StandardDeteteAsync(string Path)
    {
        var response = await httpClient.DeleteAsync(Path);
        response.EnsureSuccessStatusCode();
    }
    public async Task<ListServer?> GetServersAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException("The ApiKey or one of its properties is null.");
        }
        return await StandardGetAsync<ListServer>($"client");
    }

    public async Task<Server?> GetServerAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }
        var response = await StandardGetAsync<GetServerDetails>($"client/servers/{CurrentServerUuid}");
        return response?.Attributes;
    }

    public async Task<GetAccountDetails?> GetAccountDetailsAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }
        return await StandardGetAsync<GetAccountDetails>($"client/account");
    }

    public async Task UpdateAccountEmailAsync(string Email, string Password)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        var body = JsonSerializer.Serialize(new UpdateEmailRequest(Email, Password), jsonSettings);
        await StandardPutAsync($"client/account/email", body);
    }
    public async Task UpdateAccountPasswordAsync(string CurrentPassword, string NewPassword, string PasswordConfirmation)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var body = JsonSerializer.Serialize(new UpdatePasswordRequest(CurrentPassword, NewPassword, PasswordConfirmation), jsonSettings);
        await StandardPutAsync($"client/account/password", body);
    }
    public async Task<GetApiKeys?> GetApiKeysAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        return await StandardGetAsync<GetApiKeys>($"client/account/api-keys");
    }

    public async Task<CreateApiKeyResponse?> CreateApiKeyAsync(string Description, IEnumerable AllowedIps)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        return await StandardPostAsync<CreateApiKeyRequest, CreateApiKeyResponse>($"client/account/api-keys", new CreateApiKeyRequest(Description, AllowedIps));
    }

    public async Task DeteteApiKeyAsync(string Identifier)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        await StandardDeteteAsync($"client/account/api-keys/{Identifier}");
    }

    public async Task<GetSSHResponse?> GetSSHKeysAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        return await StandardGetAsync<GetSSHResponse>($"client/account/ssh-keys");
    }

    public async Task<CreateSSHKeyResponse?> CreateSSHKeyAsync(string Name, string PublicKey)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        return await StandardPostAsync<CreateSSHKeyRequest, CreateSSHKeyResponse>($"client/account/ssh-keys", new CreateSSHKeyRequest(Name, PublicKey));
    }

    public async Task DeteteSSHKeyAsync(string Fingerprint)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        var request = await httpClient.PostAsJsonAsync($"client/account/ssh-keys/remove", new DeleteSSHKeyRequest(Fingerprint), jsonSettings);
        request.EnsureSuccessStatusCode();
    }
}
