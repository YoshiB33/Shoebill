using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using Shoebill.ViewModels.ServerSubpages;
using SukiUI.Dialogs;

namespace Shoebill.ViewModels;

public class ServerMasterViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ISukiDialogManager _dialogManager;
    private readonly INavigationService _navigationService;
    private ViewModelBase? _currentPage;
    private string _serverName = "ServerName";


    public ServerMasterViewModel(INavigationService navigationService, IApiService apiService,
        IEnumerable<ServerViewModelBase> subPages, ISukiDialogManager dialogManager)
    {
        Pages = new AvaloniaList<ServerViewModelBase>(subPages.OrderBy(x => x.Index).ThenBy(x => x.DisplayName));
        _navigationService = navigationService;
        _apiService = apiService;
        _dialogManager = dialogManager;

        navigationService.NavigationRequested += OnNavigatedTo;

        GoBackCommand = ReactiveCommand.Create(GoBack);
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; set; }

    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        set
        {
            this.RaiseAndSetIfChanged(ref _currentPage, value);
            if (value is not null) _navigationService.MasterNavigationRequested?.Invoke(value.GetType());
        }
    }

    public string ServerName
    {
        get => _serverName;
        set => this.RaiseAndSetIfChanged(ref _serverName, value);
    }

    public IAvaloniaReadOnlyList<ServerViewModelBase> Pages { get; }

    private async void OnNavigatedTo(Type page)
    {
        if (page != typeof(ServerMasterViewModel)) return;

        Server? currentServer = null;
        try
        {
            currentServer = await _apiService.GetServerAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);

            _dialogManager.CreateDialog()
                .WithTitle($"Error found: {(int?)ex.StatusCode}")
                .WithContent($"Found a error while finding the server details: {ex.Message}\n {ex.StackTrace}")
                .OfType(NotificationType.Error)
                .Dismiss().ByClickingBackground()
                .TryShow();

            _navigationService.NavigateBack();
        }

        if (currentServer is null) return;

        ServerName = currentServer.Name;
        _apiService.CurrentServer = currentServer;
    }

    private void GoBack()
    {
        _navigationService.NavigateBack();
    }
}