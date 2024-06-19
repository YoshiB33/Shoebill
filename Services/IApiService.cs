using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.Responses;

namespace Shoebill.Services;

public interface IApiService
{
    public string? CurrentServerUuid { get; set; }
    public void SetApiKey(ApiKey? apikey);
    public Task<ListServer?> GetServersAsync();
}
