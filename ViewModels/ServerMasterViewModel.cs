using System;
using System.Reactive;
using ReactiveUI;
using Shoebill.Services;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    private string _serverName = "ServerName";
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;

    public string ServerName
    {
        get => _serverName;
        set => this.RaiseAndSetIfChanged(ref _serverName, value);
    }

    public ServerMasterViewModel(INavigationService navigationService, IApiService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;

        navigationService.NavigationRequested += OnNavigatedTo;

        GoBackCommand = ReactiveCommand.Create(GoBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page == typeof(ServerMasterViewModel))
        {
            var currentServer = await _apiService.GetServerAsync();
            _apiService.CurrentServer = currentServer;
            if (currentServer is not null)
            {
                ServerName = currentServer.Name;
            }
        }
    }

    private void GoBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>(false);
}
