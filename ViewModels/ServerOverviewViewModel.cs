using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using Avalonia.Controls.Notifications;
using ReactiveUI;
using Shoebill.Models.Api.Responses;
using Shoebill.Models.Api.Schemas;
using Shoebill.Services;
using SukiUI.Dialogs;

namespace Shoebill.ViewModels;

public class ServerOverviewViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private readonly ISukiDialogManager _dialogManager;
    private readonly INavigationService _navigationService;
    private bool _isLoading;

    public ServerOverviewViewModel(INavigationService navigationService, IApiService apiService,
        ISukiDialogManager dialogManager)
    {
        _navigationService = navigationService;
        _apiService = apiService;
        _dialogManager = dialogManager;

        navigationService.NavigationRequested += OnNavigatedTo;

        NavigateServerCommand = ReactiveCommand.Create<string>(NavigateServer);
        NavigateAccountCommand = ReactiveCommand.Create(NavigateAccount);
    }

    public ReactiveCommand<string, Unit> NavigateServerCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NavigateAccountCommand { get; set; }
    public ObservableCollection<Server> Servers { get; set; } = [];

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private async void OnNavigatedTo(Type page)
    {
        if (page != typeof(ServerOverviewViewModel)) return;

        Servers.Clear();
        IsLoading = true;
        ListServer? servers = null;
        try
        {
            servers = await _apiService.GetServersAsync();
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

        if (servers != null)
        {
            foreach (var server in servers.Data) Servers.Add(server.Attributes);

            IsLoading = false;
        }

        _apiService.CurrentServer = null;
        _apiService.CurrentServerUuid = null;
    }

    private void NavigateServer(string uuid)
    {
        _apiService.CurrentServerUuid = uuid;
        _navigationService.RequestNaviagtion<ServerMasterViewModel>();
    }

    private void NavigateAccount() =>
        _navigationService.RequestNaviagtion<ServerAccountViewModel>();
}