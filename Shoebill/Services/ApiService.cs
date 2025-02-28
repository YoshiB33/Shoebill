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
    private readonly JsonSerializerOptions _jsonSettings = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private ApiKey? ApiKey { get; set; }
    public string? CurrentServerUuid { get; set; }
    public Server? CurrentServer { get; set; }
    public GetAccountAttributes? CurrentAccount { get; set; }

    public void SetApiKey(ApiKey? apiKey)
    {
        ApiKey = apiKey;
    }

    public async Task<ListServer?> GetServersAsync()
    {
        if (ApiKey?.Key is null) throw new ArgumentNullException();

        var servers = await StandardGetAsync<ListServer>($"https://{ApiKey.ServerAdress}/api/client");
        return servers;
    }

    public async Task<Server?> GetServerAsync()
    {
        if (ApiKey?.Key is null) throw new ArgumentNullException(nameof(ApiKey));
        if (CurrentServerUuid is null) throw new ArgumentNullException(nameof(CurrentServerUuid));

        var response =
            await StandardGetAsync<GetServerDetailsResponse>(
                $"https://{ApiKey.ServerAdress}/api/client/servers/{CurrentServerUuid}");
        return response?.Attributes;
    }

    public async Task<GetAccountDetails?> GetAccountDetailsAsync()
    {
        if (ApiKey?.Key is null) throw new ArgumentNullException(nameof(ApiKey));

        return await StandardGetAsync<GetAccountDetails>($"https://{ApiKey.ServerAdress}/api/client/account");
    }

    public async Task UpdateAccountEmailAsync(string email, string password)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        var body = JsonSerializer.Serialize(new UpdateEmailRequest(email, password), _jsonSettings);
        await StandardPutAsync($"https://{ApiKey.ServerAdress}/api/client/account/email", body);
    }

    public async Task UpdateAccountPasswordAsync(string currentPassword, string newPassword,
        string passwordConfirmation)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        var body = JsonSerializer.Serialize(
            new UpdatePasswordRequest(currentPassword, newPassword, passwordConfirmation), _jsonSettings);
        await StandardPutAsync($"https://{ApiKey.ServerAdress}/api/client/account/password", body);
    }

    public async Task<GetApiKeys?> GetApiKeysAsync()
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        return await StandardGetAsync<GetApiKeys>($"https://{ApiKey.ServerAdress}/api/client/account/api-keys");
    }

    public async Task<CreateApiKeyResponse?> CreateApiKeyAsync(string description, IEnumerable allowedIps)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        var body = JsonSerializer.Serialize(new CreateApiKeyRequest(description, allowedIps), _jsonSettings);
        return await StandardPostAsync<CreateApiKeyResponse>(
            $"https://{ApiKey.ServerAdress}/api/client/account/api-keys", body);
    }

    public async Task DeleteApiKeyAsync(string identifier)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        await StandardDeleteAsync($"https://{ApiKey.ServerAdress}/api/client/account/api-keys/{identifier}");
    }

    public async Task<GetSshResponse?> GetSshKeysAsync()
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        return await StandardGetAsync<GetSshResponse>($"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys");
    }

    public async Task<CreateSshKeyResponse?> CreateSshKeyAsync(string name, string publicKey)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        var body = JsonSerializer.Serialize(new CreateSshKeyRequest(name, publicKey), _jsonSettings);
        return await StandardPostAsync<CreateSshKeyResponse>(
            $"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys", body);
    }

    public async Task DeleteSshKeyAsync(string fingerprint)
    {
        if (ApiKey?.Key is null || ApiKey.Name is null) throw new ArgumentException(nameof(ApiKey));

        await httpClient.PostAsJsonAsync($"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys/remove",
            new DeleteSSHKeyRequest(fingerprint), _jsonSettings);
    }

    public async Task<GetWebsocketResponse?> GetWebsocketAsync()
    {
        if (ApiKey?.Key is null || ApiKey.Name is null)
            throw new ArgumentException(nameof(ApiKey));

        if (string.IsNullOrWhiteSpace(CurrentServerUuid))
            throw new ArgumentException(nameof(CurrentServerUuid));

        return await StandardGetAsync<GetWebsocketResponse>(
            $"https://{ApiKey.ServerAdress}/api/client/servers/{CurrentServerUuid}/websocket");
    }

    private async Task<T?> StandardGetAsync<T>(string path)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        using var response = await httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var test = JsonSerializer.Deserialize<T>(content, _jsonSettings);
        return test;
    }

    private async Task StandardPutAsync(string path, string body)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(path),
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<T?> StandardPostAsync<T>(string path, string body)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(path),
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var deJson = await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync());
        return deJson;
    }

    private async Task StandardDeleteAsync(string path)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(path),
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        };
        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    ~ApiService()
    {
        httpClient.CancelPendingRequests();
        httpClient.Dispose();
    }
}