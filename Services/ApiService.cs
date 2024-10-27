﻿using System;
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
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException("The ApiKey or one of its properties is null.");
        }

        return await StandardGetAsync<ListServer>($"https://{ApiKey.ServerAdress}/api/client");
    }

    public async Task<Server?> GetServerAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }

        var response =
            await StandardGetAsync<GetServerDetails>(
                $"https://{ApiKey.ServerAdress}/api/client/servers/{CurrentServerUuid}");
        return response?.Attributes;
    }

    public async Task<GetAccountDetails?> GetAccountDetailsAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }

        return await StandardGetAsync<GetAccountDetails>($"https://{ApiKey.ServerAdress}/api/client/account");
    }

    public async Task UpdateAccountEmailAsync(string email, string password)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        var body = JsonSerializer.Serialize(new UpdateEmailRequest(email, password), _jsonSettings);
        await StandardPutAsync($"https://{ApiKey.ServerAdress}/api/client/account/email", body);
    }

    public async Task UpdateAccountPasswordAsync(string currentPassword, string newPassword,
        string passwordConfirmation)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var body = JsonSerializer.Serialize(
            new UpdatePasswordRequest(currentPassword, newPassword, passwordConfirmation), _jsonSettings);
        await StandardPutAsync($"https://{ApiKey.ServerAdress}/api/client/account/password", body);
    }

    public async Task<GetApiKeys?> GetApiKeysAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        return await StandardGetAsync<GetApiKeys>($"https://{ApiKey.ServerAdress}/api/client/account/api-keys");
    }

    public async Task<CreateApiKeyResponse?> CreateApiKeyAsync(string description, IEnumerable allowedIps)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        var body = JsonSerializer.Serialize(new CreateApiKeyRequest(description, allowedIps), _jsonSettings);
        return await StandardPostAsync<CreateApiKeyResponse>(
            $"https://{ApiKey.ServerAdress}/api/client/account/api-keys", body);
    }

    public async Task DeteteApiKeyAsync(string identifier)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        await StandardDeleteAsync($"https://{ApiKey.ServerAdress}/api/client/account/api-keys/{identifier}");
    }

    public async Task<GetSSHResponse?> GetSshKeysAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        return await StandardGetAsync<GetSSHResponse>($"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys");
    }

    public async Task<CreateSSHKeyResponse?> CreateSshKeyAsync(string name, string publicKey)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        var body = JsonSerializer.Serialize(new CreateSSHKeyRequest(name, publicKey), _jsonSettings);
        return await StandardPostAsync<CreateSSHKeyResponse>(
            $"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys", body);
    }

    public async Task DeteteSshKeyAsync(string fingerprint)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }

        var client = new HttpClient();
        await client.PostAsJsonAsync($"https://{ApiKey.ServerAdress}/api/client/account/ssh-keys/remove",
            new DeleteSSHKeyRequest(fingerprint), _jsonSettings);
    }

    public async Task<T?> StandardGetAsync<T>(string path)
    {
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        using var response = await httpClient.GetAsync(path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task StandardPutAsync(string path, string body)
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

    public async Task<T?> StandardPostAsync<T>(string path, string body)
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

    public async Task StandardDeleteAsync(string path)
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