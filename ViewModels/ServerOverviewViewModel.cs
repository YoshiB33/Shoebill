﻿using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using ReactiveUI;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

public class ServerOverviewViewModel : ViewModelBase
{
    private bool _isLoading = false;
    public ReactiveCommand<Unit, Unit> NavigateBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    public ReactiveCommand<string, Unit> NavigateServerCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateAccountCommand { get; set; }
    public ObservableCollection<Server> Servers { get; set; } = [];
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }
    public ServerOverviewViewModel(INavigationService navigationService, IApiService apiService)
    {
        _navigationService = navigationService;
        _apiService = apiService;

        navigationService.NavigationRequested += OnNavigatedTo;

        NavigateBackCommand     = ReactiveCommand.Create(NavigateBack);
        NavigateSettingsCommand = ReactiveCommand.Create(NavigateSettings);
        NavigateServerCommand   = ReactiveCommand.Create<string>(NavigateServer);
        NavigateAccountCommand  = ReactiveCommand.Create(NavigateAccount);
    }

    private async void OnNavigatedTo(Type page)
    {
        Servers.Clear();
        IsLoading = true;
        if (page == typeof(ServerOverviewViewModel))
        {
            ListServer? servers = null;
            try
            {
                servers = await _apiService.GetServersAsync();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                
                SukiHost.ShowMessageBox(new SukiUI.Models.MessageBoxModel($"Error found: {(int?)ex.StatusCode}", $"Found a error while finding the server details: {ex.Message}\n {ex.StackTrace}", SukiUI.Enums.NotificationType.Error),true);

                _navigationService.NavigateBack();
            }

            if (servers is not null && servers.Data is not null)
            {
                foreach (var server in servers.Data)
                {
                    Servers.Add(server.Attributes);
                }
                IsLoading = false;   
            }

            _apiService.CurrentServer = null;
            _apiService.CurrentServerUuid = null;
        }
    }

    private void NavigateBack() =>
        _navigationService.NavigateBack();
    private void NavigateSettings() =>
        _navigationService.RequestNaviagtion<SettingsViewModel>(false);
    
    private void NavigateServer(string uuid)
    {
        _apiService.CurrentServerUuid = uuid;
        _navigationService.RequestNaviagtion<ServerMasterViewModel>(false);
    }
    private void NavigateAccount() =>
        _navigationService.RequestNaviagtion<ServerAccountViewModel>(false);
}
