using System.ComponentModel.DataAnnotations;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models;
using Shoebill.Services;
using SukiUI.Dialogs;

namespace Shoebill.ViewModels.Dialogs;

public class CreateAccountViewModel : ViewModelBase
{
    private readonly ObservableAsPropertyHelper<bool> _canCreateAccount;
    private readonly ISukiDialog _dialog;
    private readonly ISettingsService _settingsService;
    private string _headline = "Add new api key";
    private bool _isApplicationSelected;
    private bool _isClientSelected;
    private bool _isEntering;
    private string _serverApiKey = string.Empty;
    private string _serverName = string.Empty;
    private string _serverUrl = string.Empty;

    public CreateAccountViewModel(ISettingsService settingsService, ISukiDialog dialog, ApiKey? editApiKey = null)
    {
        var apiKeys = settingsService.GetAllApiKeys();
        _settingsService = settingsService;
        _dialog = dialog;

        if (editApiKey is not null)
        {
            var canCreateUser = this.WhenAnyValue(
                    x => x.ServerUrl, x => x.ServerName, x => x.ServerApiKey, x => x.IsClientSelected,
                    x => x.IsApplicationSelected,
                    (url, name, key, clientSelected, applicationSelected) =>
                        !string.IsNullOrWhiteSpace(name) &&
                        !string.IsNullOrWhiteSpace(key) &&
                        !string.IsNullOrWhiteSpace(url) &&
                        (applicationSelected || clientSelected))
                .ToProperty(this, x => x.CanCreateAccount, out _canCreateAccount);
        }
        else
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var canCreateUser = this.WhenAnyValue(
                    x => x.ServerUrl, x => x.ServerName, x => x.ServerApiKey, x => x.IsClientSelected,
                    x => x.IsApplicationSelected,
                    (url, name, key, clientSelected, applicationSelected) =>
                        !string.IsNullOrWhiteSpace(name) &&
                        !string.IsNullOrWhiteSpace(key) &&
                        !string.IsNullOrWhiteSpace(url) &&
                        !apiKeys.Exists(x => x.Key == key) &&
                        !apiKeys.Exists(x => x.Name == name) &&
                        (applicationSelected || clientSelected))
                .ToProperty(this, x => x.CanCreateAccount, out _canCreateAccount);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }


        EnterCommand = ReactiveCommand.Create(Enter);
        CancelCommand = ReactiveCommand.Create(Cancel);

        if (editApiKey != null && editApiKey.Key != null &&
            (editApiKey.ApiType == ApiTypes.Client || editApiKey.ApiType == ApiTypes.Application) &&
            editApiKey.Name != null && editApiKey.ServerAdress != null)
        {
            ServerApiKey = editApiKey.Key;
            ServerName = editApiKey.Name;
            ServerUrl = editApiKey.ServerAdress;
            if (editApiKey.ApiType == ApiTypes.Client)
            {
                IsClientSelected = true;
            }
            else
            {
                IsApplicationSelected = true;
            }

            ApiKey = editApiKey;
            Headline = "Edit api key";
        }
    }

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

    public bool CanCreateAccount => _canCreateAccount.Value;
    public ApiKey? ApiKey { get; set; }

    private async void Enter()
    {
        if (ApiKey is not null)
        {
            await _settingsService.RemoveApiAsync(ApiKey);
        }

        IsEntering = true;
        if (IsClientSelected)
        {
            await _settingsService.WriteApiKeyAsync(new ApiKey
            {
                Name = ServerName,
                ServerAdress = ServerUrl,
                Key = ServerApiKey,
                ApiType = ApiTypes.Client
            });
        }
        else
        {
            await _settingsService.WriteApiKeyAsync(new ApiKey
            {
                Name = ServerName,
                ServerAdress = ServerUrl,
                Key = ServerApiKey,
                ApiType = ApiTypes.Application
            });
        }

        IsEntering = false;
        _dialog.Dismiss();
    }

    private void Cancel()
    {
        _dialog.Dismiss();
    }
}