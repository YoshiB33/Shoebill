using System;
using System.Net.WebSockets;
using System.Reactive;
using System.Threading;
using Material.Icons;
using ReactiveUI;
using Shoebill.Models.Api.Responses;
using Shoebill.Services;

namespace Shoebill.ViewModels.ServerSubpages;

public class ServerConsoleViewModel : ServerViewModelBase
{
    private readonly IApiService _apiService;

    private readonly ApiWsClient _ws;
    private bool? _isRestartButtonActive;
    private bool? _isStartButtonActive;
    private bool? _isStopButtonActive;
    private string? _uptimeText;

    public ServerConsoleViewModel(IApiService apiService, INavigationService navigationService)
    {
        _ws = new ApiWsClient();
        _apiService = apiService;

        StartServerCommand = ReactiveCommand.Create(StartServer);
        StopServerCommand = ReactiveCommand.Create(StopServer);
        RestartServerCommand = ReactiveCommand.Create(RestartServer);

        navigationService.NavigationRequested += OnNavigated;
        _ws.AuthSuccess += () => { Console.WriteLine("Auth Success"); };
        _ws.TokenExpired += ReAuth;
        _ws.TokenExpiring += ReAuth;
        _ws.Stats += ProcessStats;
    }

    public override MaterialIconKind Icon => MaterialIconKind.Console;
    public override int Index => 1;
    public override string DisplayName => "Console";

    public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
    public ReactiveCommand<Unit, Unit> StopServerCommand { get; }
    public ReactiveCommand<Unit, Unit> RestartServerCommand { get; }

    public string? UptimeText
    {
        get => _uptimeText;
        set => this.RaiseAndSetIfChanged(ref _uptimeText, value);
    }

    public bool? IsStartButtonActive
    {
        get => _isStartButtonActive;
        set => this.RaiseAndSetIfChanged(ref _isStartButtonActive, value);
    }

    public bool? IsStopButtonActive
    {
        get => _isStopButtonActive;
        set => this.RaiseAndSetIfChanged(ref _isStopButtonActive, value);
    }

    public bool? IsRestartButtonActive
    {
        get => _isRestartButtonActive;
        set => this.RaiseAndSetIfChanged(ref _isRestartButtonActive, value);
    }

    private async void OnNavigated(Type page)
    {
        if (page != typeof(ServerMasterViewModel))
        {
            if (_ws.State == WebSocketState.Open) await _ws.CloseAsync(CancellationToken.None);
            return;
        }

        var wsCredentials = await _apiService.GetWebsocketAsync();
        if (wsCredentials == null) return;

        Console.WriteLine("Connected to WS");
        _ws.Connect(new Uri(wsCredentials.Data.Socket), wsCredentials.Data.Token);
        await _ws.ReceiveAsync(CancellationToken.None);
    }

    private async void ReAuth()
    {
        var credentials = await _apiService.GetWebsocketAsync();
        if (credentials == null) return;
        _ws.Auth(credentials.Data.Token);
    }

    private void ProcessStats(StatsWsResponse stats)
    {
        if (stats.State == "running")
        {
            UptimeText = TimeSpan.FromMilliseconds(stats.Uptime).ToString(@"hh\:mm\:ss");
            IsStartButtonActive = false;
            IsStopButtonActive = true;
            IsRestartButtonActive = true;
        }
        else
        {
            UptimeText = stats.State[..1].ToUpper() + stats.State[1..];
            IsStopButtonActive = false;
            IsStartButtonActive = true;
            _isRestartButtonActive = true;
        }

        if (stats.State == "starting")
        {
            IsStartButtonActive = false;
            IsRestartButtonActive = true;
            IsStopButtonActive = true;
        }
        else if (stats.State == "stopping")
        {
            IsStartButtonActive = true;
            IsRestartButtonActive = true;
            IsStopButtonActive = false;
        }
    }

    private void StartServer()
    {
        _ws.SetState(PowerAction.Start);
    }

    private void StopServer()
    {
        _ws.SetState(PowerAction.Stop);
    }

    private void RestartServer()
    {
        _ws.SetState(PowerAction.Restart);
    }

    ~ServerConsoleViewModel()
    {
        _ws.Dispose();
    }
}