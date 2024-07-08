using System;
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

    public void SetApiKey(ApiKey? apiKey)
    {
        ApiKey = apiKey;
    }

    public async Task<ListServer?> GetServersAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey.Key);
        var response = await client.GetAsync($"https://{ApiKey.ServerAdress}/api/client");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ListServer>();
    }

    public async Task<Server?> GetServerAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey.Key);
        var response = await client.GetAsync($"https://{ApiKey.ServerAdress}/api/client/servers/{CurrentServerUuid}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<GetServerDetails>();
        return json?.Attributes;
    }

    public async Task<GetAccountDetails?> GetAccountDetailsAsync()
    {
        if (ApiKey is null || ApiKey.Key is null || ApiKey.Key is null)
        {
            throw new ArgumentNullException(nameof(ApiKey));
        }
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey.Key);
        var response = await client.GetAsync($"https://{ApiKey.ServerAdress}/api/client/account");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<GetAccountDetails>();
        return json;
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
        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey.Key);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Put,
            RequestUri = new Uri($"https://{ApiKey.ServerAdress}/api/client/account/email"),
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        var response = await client.SendAsync(request);
        System.Console.WriteLine(await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
    }
}
