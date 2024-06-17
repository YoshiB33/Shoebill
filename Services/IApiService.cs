using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.ListApiModel;

namespace Shoebill.Services;

public interface IApiService
{
    public void SetApiKey(ApiKey? apikey);
    public Task<ListApiModel?> GetServersAsync();
}
