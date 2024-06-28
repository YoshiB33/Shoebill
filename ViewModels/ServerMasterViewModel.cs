using System;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using SukiUI.Controls;

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
            Server? currentServer = null;
            try
            {
                currentServer = await _apiService.GetServerAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                
                SukiHost.ShowMessageBox(new SukiUI.Models.MessageBoxModel($"Error found: {(int?)ex.StatusCode}", $"Found a error while finding the server details: {ex.Message}\n {ex.StackTrace}", SukiUI.Enums.NotificationType.Error),true);

                _navigationService.NavigateBack();
            }
            if (currentServer is not null)
            {
                ServerName = currentServer.Name;
                _apiService.CurrentServer = currentServer;
            }
        }
    }

    private void GoBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>(false);
}
