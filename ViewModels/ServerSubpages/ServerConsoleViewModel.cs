using System;
using System.Net.WebSockets;
using System.Threading;
using Material.Icons;
using Shoebill.Services;

namespace Shoebill.ViewModels.ServerSubpages;

public class ServerConsoleViewModel : ServerViewModelBase
{
    public ServerConsoleViewModel(IApiService apiService, INavigationService navigationService)
    {
        Ws = new ApiWsClient();
        ApiService = apiService;
        NavigationService = navigationService;

        NavigationService.NavigationRequested += OnNavigated;
        Ws.AuthSuccess += () => { Console.WriteLine("Auth Success"); };
    }

    public override MaterialIconKind Icon => MaterialIconKind.Console;
    public override int Index => 1;
    public override string DisplayName => "Console";

    private ApiWsClient Ws { get; }
    private IApiService ApiService { get; }
    private INavigationService NavigationService { get; }

    private async void OnNavigated(Type page)
    {
        if (page != typeof(ServerMasterViewModel))
        {
            if (Ws.State == WebSocketState.Open) Ws.Close();
            return;
        }

        var wsCredentials = await ApiService.GetWebsocketAsync();
        if (wsCredentials == null) return;

        Console.WriteLine("Connected to WS");
        Ws.Connect(new Uri(wsCredentials.Data.Socket), wsCredentials.Data.Token);
        await Ws.ReceiveAsync(CancellationToken.None);
    }

    ~ServerConsoleViewModel()
    {
        Ws.Dispose();
    }
}