using System;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Reactive;
using System.Threading;
using Avalonia.Input;
using ByteSizeLib;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Material.Icons;
using ReactiveUI;
using Shoebill.Models.Api.Responses;
using Shoebill.Services;
using SukiUI.Dialogs;
using static SkiaSharp.SKColor;

namespace Shoebill.ViewModels.ServerSubpages;

public class ServerConsoleViewModel : ServerViewModelBase
{
    private readonly IApiService _apiService;

    private readonly ApiWsClient _ws;
    private string _consoleText = string.Empty;
    private string _cpuText = "NO DATA";
    private string _diskText = "NO DATA";
    private bool? _isRestartButtonActive;
    private bool? _isStartButtonActive;
    private bool? _isStopButtonActive;
    private string _memoryMText = "NO DATA";
    private string _memoryUText = "NO DATA";
    private string _networkIn = "NO DATA";
    private string _networkOut = "NO DATA";
    private string _uptimeText = "NO DATA";
    private string _commandContent = string.Empty;

    public ServerConsoleViewModel(IApiService apiService, INavigationService navigationService, ISukiDialogManager dialogManager)
    {
        _ws = new ApiWsClient();
        _apiService = apiService;
        _dialogManager = dialogManager;

        CpuSeries =
        [
            new LineSeries<DateTimePoint>(CpuValues)
            {
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        ];
        MemorySeries =
        [
            new LineSeries<DateTimePoint>(MemoryValues)
            {
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0.5
            }
        ];

        StartServerCommand = ReactiveCommand.Create(StartServer);
        StopServerCommand = ReactiveCommand.Create(StopServer);
        RestartServerCommand = ReactiveCommand.Create(RestartServer);
        SendCommandCommand=ReactiveCommand.Create<KeyEventArgs>(SendCommand);

        navigationService.NavigationRequested += OnNavigated;
        _ws.AuthSuccess += () => { };
        _ws.TokenExpired += ReAuth;
        _ws.TokenExpiring += ReAuth;
        _ws.Stats += ProcessStats;
        _ws.ConsoleOutput += HandleConsoleOutput;
        _ws.DaemonMessage += HandleConsoleOutput;
        _ws.DaemonError += HandleConsoleOutput;
        _ws.InstallOutput += HandleConsoleOutput;
        _ws.TransferLogs += HandleConsoleOutput;
        _ws.TransferStatus += HandleConsoleOutput;
    }

    public override MaterialIconKind Icon => MaterialIconKind.Console;
    public override int Index => 1;
    public override string DisplayName => "Console";

    public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
    public ReactiveCommand<Unit, Unit> StopServerCommand { get; }
    public ReactiveCommand<Unit, Unit> RestartServerCommand { get; }
    public ReactiveCommand<KeyEventArgs, Unit> SendCommandCommand { get; }

    public ObservableCollection<ISeries> CpuSeries { get; set; }
    public ObservableCollection<ISeries> MemorySeries { get; set; }
    private ObservableCollection<DateTimePoint> CpuValues { get; } = [];
    private ObservableCollection<DateTimePoint> MemoryValues { get; } = [];

    private readonly ISukiDialogManager _dialogManager;

    public ICartesianAxis[] CpuAxis { get; set; } =
    [
        new Axis
        {
            Labeler = percentage => percentage.ToString("P1"),
            Name = "CPU Load",
            NamePaint = new SolidColorPaint(Parse("#777"))
        }
    ];

    public ICartesianAxis[] MemoryAxis { get; set; } =
    [
        new Axis
        {
            Labeler = byteSize => ByteSize.FromBytes(byteSize).ToString(),
            Name = "Memory Usage",
            NamePaint = new SolidColorPaint(Parse("#777"))
        }
    ];

    public ICartesianAxis[] XAxes { get; set; } =
    [
        new DateTimeAxis(TimeSpan.FromSeconds(1), date => date.ToString("HH:mm:ss"))
    ];

    public string UptimeText
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

    public string CpuText
    {
        get => _cpuText;
        set => this.RaiseAndSetIfChanged(ref _cpuText, value);
    }

    public string MemoryUText
    {
        get => _memoryUText;
        set => this.RaiseAndSetIfChanged(ref _memoryUText, value);
    }

    public string MemoryMText
    {
        get => _memoryMText;
        set => this.RaiseAndSetIfChanged(ref _memoryMText, value);
    }

    public string DiskText
    {
        get => _diskText;
        set => this.RaiseAndSetIfChanged(ref _diskText, value);
    }

    public string NetworkIn
    {
        get => _networkIn;
        set => this.RaiseAndSetIfChanged(ref _networkIn, value);
    }

    public string NetworkOut
    {
        get => _networkOut;
        set => this.RaiseAndSetIfChanged(ref _networkOut, value);
    }

    public string ConsoleText
    {
        get => _consoleText;
        set => this.RaiseAndSetIfChanged(ref _consoleText, value);
    }

    public string CommandContent
    {
        get => _commandContent;
        set => this.RaiseAndSetIfChanged(ref _commandContent, value);
    }

    private async void OnNavigated(Type page)
    {
        try
        {
            if (page != typeof(ServerMasterViewModel))
            {
                if (_ws.State == WebSocketState.Open) await _ws.CloseAsync(CancellationToken.None);
                return;
            }

            CpuValues.Clear();
            MemoryValues.Clear();

            var wsCredentials = await _apiService.GetWebsocketAsync();
            if (wsCredentials == null) return;

            Console.WriteLine("Connected to WS");
            _ws.Connect(new Uri(wsCredentials.Data.Socket), wsCredentials.Data.Token);
            await _ws.ReceiveAsync(CancellationToken.None);
        }
        catch (Exception e)
        {
            _dialogManager.CreateDialog()
                .WithTitle($"Error navigating to {nameof(ServerConsoleViewModel)}.")
                .WithContent(e.Message)
                .TryShow();
        }
    }

    private async void ReAuth()
    {
        try
        {
            var credentials = await _apiService.GetWebsocketAsync();
            if (credentials == null) return;
            _ws.Auth(credentials.Data.Token);
        }
        catch (Exception e)
        {
            _dialogManager.CreateDialog()
                .WithTitle("Error authenticating...")
                .WithContent(e.Message)
                .TryShow();
        }
    }

    private void ProcessStats(StatsWsResponse stats)
    {
        switch (stats.State)
        {
            case "starting":
                // Code for the state of the status buttons and the uptime box.
                IsStartButtonActive = false;
                IsRestartButtonActive = true;
                IsStopButtonActive = true;
                UptimeText = stats.State[..1].ToUpper() + stats.State[1..];

                // Code for the CPU, memory and disk boxes
                MemoryUText = ByteSize.FromBytes(stats.Memory_bytes).ToString();
                MemoryMText = stats.Memory_limit_bytes == 0
                    ? "\u221e"
                    : ByteSize.FromBytes(stats.Memory_limit_bytes).ToString();
                CpuText = $"{Math.Round(stats.Cpu_absolute, 2)}%";
                DiskText = ByteSize.FromBytes(stats.Disk_bytes).ToString();

                // Code for the network
                NetworkIn = ByteSize.FromBytes(stats.Network.Rx_bytes).ToString();
                NetworkOut = ByteSize.FromBytes(stats.Network.Tx_bytes).ToString();
                break;
            case "stopping":
                // Code for the state of the status buttons and the uptime box.
                IsStartButtonActive = true;
                IsRestartButtonActive = true;
                IsStopButtonActive = false;
                UptimeText = stats.State[..1].ToUpper() + stats.State[1..];

                // Code for the CPU, memory and disk boxes
                MemoryUText = ByteSize.FromBytes(stats.Memory_bytes).ToString();
                MemoryMText = stats.Memory_limit_bytes == 0
                    ? "\u221e"
                    : ByteSize.FromBytes(stats.Memory_limit_bytes).ToString();
                CpuText = $"{Math.Round(stats.Cpu_absolute, 2)}%";
                DiskText = ByteSize.FromBytes(stats.Disk_bytes).ToString();

                // Code for the network
                NetworkIn = ByteSize.FromBytes(stats.Network.Rx_bytes).ToString();
                NetworkOut = ByteSize.FromBytes(stats.Network.Tx_bytes).ToString();
                break;
            case "offline":
                // Code for the state of the status buttons and the uptime box.
                IsStopButtonActive = false;
                IsStartButtonActive = true;
                _isRestartButtonActive = true;
                UptimeText = stats.State[..1].ToUpper() + stats.State[1..];

                // Code for the CPU, memory and disk boxes
                MemoryUText = "Offline";
                MemoryMText = stats.Memory_limit_bytes == 0
                    ? "\u221e"
                    : ByteSize.FromBytes(stats.Memory_limit_bytes).ToString();
                CpuText = "Offline";
                DiskText = ByteSize.FromBytes(stats.Disk_bytes).ToString();

                // Code for the network
                NetworkIn = "Offline";
                NetworkOut = "Offline";
                break;
            case "running":
                // Code for the state of the status buttons and the uptime box.
                IsStartButtonActive = false;
                IsStopButtonActive = true;
                IsRestartButtonActive = true;
                UptimeText = TimeSpan.FromMilliseconds(stats.Uptime).ToString(@"hh\:mm\:ss");

                // Code for the CPU, memory and disk boxes
                MemoryUText = ByteSize.FromBytes(stats.Memory_bytes).ToString();
                MemoryMText = stats.Memory_limit_bytes == 0
                    ? "\u221e"
                    : ByteSize.FromBytes(stats.Memory_limit_bytes).ToString();
                CpuText = $"{Math.Round(stats.Cpu_absolute, 2)}%";
                DiskText = ByteSize.FromBytes(stats.Disk_bytes).ToString();

                // Code for the network
                NetworkIn = ByteSize.FromBytes(stats.Network.Rx_bytes).ToString();
                NetworkOut = ByteSize.FromBytes(stats.Network.Tx_bytes).ToString();
                break;
        }

        // Code for the graphs
        CpuValues.Add(new DateTimePoint { DateTime = DateTime.Now, Value = Math.Round(stats.Cpu_absolute, 2) / 100 });
        if (CpuValues[0].DateTime <= DateTime.Now - TimeSpan.FromSeconds(90))
            CpuValues.RemoveAt(0);

        MemoryValues.Add(new DateTimePoint { DateTime = DateTime.Now, Value = stats.Memory_bytes });
        if (MemoryValues[0].DateTime <= DateTime.Now - TimeSpan.FromSeconds(90))
            MemoryValues.RemoveAt(0);
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

    private void HandleConsoleOutput(string output)
    {
        ConsoleText += output + "\n";
    }

    private void SendCommand(KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        try
        {
            _ws.SendCommand(CommandContent);
            CommandContent = string.Empty;
        }
        catch (Exception ex)
        {
            _dialogManager.CreateDialog()
                .WithTitle("Error sending command")
                .WithContent(ex.Message)
                .TryShow();
        }
    }

    ~ServerConsoleViewModel()
    {
        _ws.Dispose();
    }
}