using System.Threading.Tasks;
using Shoebill.Models;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;

namespace Shoebill.Services;

public interface IApiService
{
    public string? CurrentServerUuid { get; set; }
    public Server? CurrentServer { get; set; }
    public GetAccountAttributes? CurrentAccount { get; set; }
    public void SetApiKey(ApiKey? apikey);
    public Task<ListServer?> GetServersAsync();
    public Task<Server?> GetServerAsync();
    public Task<GetAccountDetails?> GetAccountDetailsAsync();
    public Task UpdateAccountEmailAsync(string Email, string Password);
}
