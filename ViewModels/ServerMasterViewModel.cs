﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using Avalonia.Collections;
using ReactiveUI;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using Shoebill.ViewModels.ServerSubpages;
using SukiUI.Controls;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    private string _serverName = "ServerName";
    private ViewModelBase? _currentPage;
    public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateSettingsCommand { get; set; }
    private readonly INavigationService _navigationService;
    private readonly IApiService _apiService;
    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            if (value is not null)
            {
                _navigationService.MasterNavigationRequested?.Invoke(value.GetType());
            }
        }
    }
    public string ServerName
    {
        get => _serverName;
        set => this.RaiseAndSetIfChanged(ref _serverName, value);
    }

    public IAvaloniaReadOnlyList<ServerViewModelBase> Pages { get; }


    public ServerMasterViewModel(INavigationService navigationService, IApiService apiService, IEnumerable<ServerViewModelBase> subPages)
    {
        Pages = new AvaloniaList<ServerViewModelBase>(subPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
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

                SukiHost.ShowMessageBox(new SukiUI.Models.MessageBoxModel($"Error found: {(int?)ex.StatusCode}", $"Found a error while finding the server details: {ex.Message}\n {ex.StackTrace}", SukiUI.Enums.NotificationType.Error), true);

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
