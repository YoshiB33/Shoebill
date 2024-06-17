using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.Responses;

namespace Shoebill.Services;

public class ApiService : IApiService
{
    private ApiKey? ApiKey { get; set; }

    public void SetApiKey(ApiKey? apiKey)
    {
        this.ApiKey = apiKey;
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
        return await client.GetFromJsonAsync<ListServer>($"https://{ApiKey.ServerAdress}/api/client");
    }
}
