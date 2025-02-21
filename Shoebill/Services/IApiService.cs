using System.Collections;
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
    public Task UpdateAccountEmailAsync(string email, string password);
    public Task UpdateAccountPasswordAsync(string currentPassword, string newPassword, string passwordConfirmation);
    public Task<GetApiKeys?> GetApiKeysAsync();
    public Task<CreateApiKeyResponse?> CreateApiKeyAsync(string description, IEnumerable allowedIps);
    public Task DeleteApiKeyAsync(string identifier);
    public Task<GetSshResponse?> GetSshKeysAsync();
    public Task<CreateSshKeyResponse?> CreateSshKeyAsync(string name, string publicKey);
    public Task DeleteSshKeyAsync(string fingerprint);
    public Task<GetWebsocketResponse?> GetWebsocketAsync();
}