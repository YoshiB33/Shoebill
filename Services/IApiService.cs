using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.Responses;

namespace Shoebill.Services;

public interface IApiService
{
    public void SetApiKey(ApiKey? apikey);
    public Task<ListServer?> GetServersAsync();
}
