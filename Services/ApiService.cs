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

public class ApiService : IApiService
{
    private ApiKey? ApiKey { get; set; }
    public string? CurrentServerUuid { get; set; }
    public Server? CurrentServer { get; set; }
    public GetAccountAttributes? CurrentAccount { get; set; } 
    private readonly JsonSerializerOptions jsonSettings = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void SetApiKey(ApiKey? apiKey)
    {
        ApiKey = apiKey;
    }

    public async Task<T?> StandardGetAsync<T>(string Path)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        var response = await client.GetAsync(Path);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
    public async Task StandardPutAsync(string Path, string Body)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey?.Key);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri(Path),
            Content = new StringContent(Body, Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
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
        var response = await StandardGetAsync<GetServerDetails>($"https://{ApiKey.ServerAdress}/api/client/{CurrentServerUuid}");
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

    public async Task UpdateAccountEmailAsync(string Email, string Password)
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Name is null)
        {
            throw new ArgumentException(nameof(ApiKey));
        }
        var settings = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var body = JsonSerializer.Serialize(new UpdateEmailRequest(Email, Password), settings);
        await StandardPutAsync($"https://{ApiKey.ServerAdress}/api/client/account/email", body);
    }
}
