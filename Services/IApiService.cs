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
    public Task UpdateAccountEmailAsync(string Email, string Password);
    public Task UpdateAccountPasswordAsync(string CurrentPassword, string NewPassword, string PasswordConfirmation);
    public Task<GetApiKeys?> GetApiKeysAsync();
    public Task<CreateApiKeyResponse?> CreateApiKeyAsync(string Description, IEnumerable AllowedIps);
    public Task DeteteApiKeyAsync(string Identifier);
    public Task<GetSSHResponse?> GetSSHKeysAsync();
    public Task<CreateSSHKeyResponse?> CreateSSHKeyAsync(string Name, string PublicKey);
    public Task DeteteSSHKeyAsync(string Fingerprint);
}
