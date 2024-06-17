using System.ComponentModel.DataAnnotations;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

public class CreateAccountViewModel : ViewModelBase
{
    private bool _isEntering = false;
    private string _serverUrl = string.Empty;
    private string _serverApiKey = string.Empty;
    private string _serverName = string.Empty;
    private string _headline = "Add new api key";
    private bool _isClientSelected = false;
    private bool _isApplicationSelected = false;
    private readonly ObservableAsPropertyHelper<bool> _canEnter;
    private ISettingsService _settingsService;

    public ReactiveCommand<Unit, Unit> EnterCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public bool IsEntering
    {
        get => _isEntering;
        set => this.RaiseAndSetIfChanged(ref _isEntering, value);
    }
    [Required]
    public string ServerName
    {
        get => _serverName;
        set => this.RaiseAndSetIfChanged(ref _serverName, value);
    }
    [Required]
    public string ServerUrl
    {
        get => _serverUrl;
        set => this.RaiseAndSetIfChanged(ref _serverUrl, value);
    }
    [Required]
    public string ServerApiKey
    {
        get => _serverApiKey;
        set => this.RaiseAndSetIfChanged(ref _serverApiKey, value);
    }
    public bool IsClientSelected
    {
        get => _isClientSelected;
        set => this.RaiseAndSetIfChanged(ref _isClientSelected, value);
    }
    public bool IsApplicationSelected
    {
        get => _isApplicationSelected;
        set => this.RaiseAndSetIfChanged(ref _isApplicationSelected, value);
    }
    public string Headline
    {
        get => _headline;
        set => this.RaiseAndSetIfChanged(ref _headline, value);
    }
    public bool CanEnter => _canEnter.Value;
    public ApiKey? ApiKey { get; set; }
    public CreateAccountViewModel(ISettingsService settingsService, ApiKey? editApiKey = null)
    {
        var apiKeys = settingsService.GetAllApiKeys();
        _settingsService = settingsService;
        if (editApiKey is not null)
        {
            var canCreateUser = this.WhenAnyValue(
                x => x.ServerUrl, x => x.ServerName, x => x.ServerApiKey, x => x.IsClientSelected, x => x.IsApplicationSelected,
                    (url, name, key, clientSelected, applicationSelected) => 
                        !string.IsNullOrWhiteSpace(name) && 
                        !string.IsNullOrWhiteSpace(key) &&
                        !string.IsNullOrWhiteSpace(url) &&
                        (applicationSelected || clientSelected))
                .ToProperty(this, x => x.CanEnter, out _canEnter);
        }
        else
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var canCreateUser = this.WhenAnyValue(
                x => x.ServerUrl, x => x.ServerName, x => x.ServerApiKey, x => x.IsClientSelected, x => x.IsApplicationSelected,
                    (url, name, key, clientSelected, applicationSelected) => 
                        !string.IsNullOrWhiteSpace(name) && 
                        !string.IsNullOrWhiteSpace(key) &&
                        !string.IsNullOrWhiteSpace(url) &&
                        !apiKeys.Exists(x => x.Key == key) &&
                        !apiKeys.Exists(x => x.Name == name) &&
                        (applicationSelected || clientSelected))
                .ToProperty(this, x => x.CanEnter, out _canEnter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }


        EnterCommand = ReactiveCommand.Create(Enter);
        CancelCommand = ReactiveCommand.Create(Cancel);

        if (editApiKey != null && editApiKey.Key != null && (editApiKey.ApiType == ApiTypes.Client || editApiKey.ApiType == ApiTypes.Application) && editApiKey.Name != null && editApiKey.ServerAdress != null)
        {
            ServerApiKey = editApiKey.Key;
            ServerName = editApiKey.Name;
            ServerUrl = editApiKey.ServerAdress;
            if (editApiKey.ApiType == ApiTypes.Client)
            {
                IsClientSelected  = true;
            }
            else
            {
                IsApplicationSelected = true;
            }
            ApiKey = editApiKey;
            Headline = "Edit api key";
        }
    }
    private async void Enter()
    {
        if (ApiKey is not null)
        {
            await _settingsService.RemoveApiAsync(ApiKey);
        }
        IsEntering = true;
        if (IsClientSelected)
        {
            await _settingsService.WriteApiKeyAsync(new ApiKey {
                Name = ServerName,
                ServerAdress = ServerUrl,
                Key = ServerApiKey,
                ApiType = ApiTypes.Client
            });   
        }
        else
        {
            await _settingsService.WriteApiKeyAsync(new ApiKey {
                Name = ServerName,
                ServerAdress = ServerUrl,
                Key = ServerApiKey,
                ApiType = ApiTypes.Application
            });
        }
        IsEntering = false;
        SukiHost.CloseDialog();
    }
    private void Cancel()
    {
        SukiHost.CloseDialog();
    }
}
