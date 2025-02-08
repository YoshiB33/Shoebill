using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Shoebill.Models.Api;
using Shoebill.Models.Api.Responses;

namespace Shoebill.Services;

/// <summary>
///     An object that is able to communicate with the pterodactyl websocket.
/// </summary>
/// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md" />
public class ApiWsClient : IApiWsClient, IDisposable
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly object _lock = new();

    private ClientWebSocket _ws = new();

    public WebSocketState State => _ws.State;

    public event Action? AuthSuccess;

    public event Action? BackupComplete;

    public event Action? BackupRestoreCompleted;

    public event Action<string>? ConsoleOutput;

    public event Action<string>? DaemonError;

    public event Action<string>? DaemonMessage;

    public event Action? InstallCompleted;

    public event Action<string>? InstallOutput;

    public event Action? InstallStarted;

    public event Action<string>? JwtError;

    public event Action<StatsWsResponse>? Stats;

    public event Action<PowerStatus>? Status;

    public event Action? TokenExpired;

    public event Action? TokenExpiring;

    public event Action<string>? TransferLogs;

    public event Action<string>? TransferStatus;

    public void Auth(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));

        var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("auth", [token])));
        var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
        lock (_lock)
        {
            _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public void SetState(PowerAction action)
    {
        switch (action)
        {
            case PowerAction.Start:
            {
                var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("set state", ["start"])));
                var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
                lock (_lock)
                {
                    _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
                break;
            case PowerAction.Stop:
            {
                var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("set state", ["stop"])));
                var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
                lock (_lock)
                {
                    _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
                break;
            case PowerAction.Restart:
            {
                var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("set state", ["restart"])));
                var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
                lock (_lock)
                {
                    _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
                break;
            case PowerAction.Kill:
            {
                var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("set state", ["kill"])));
                var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
                lock (_lock)
                {
                    _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void SendCommand(string command)
    {
        var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("send command", [command])));
        var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
        lock (_lock)
        {
            _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public void RequestLogs()
    {
        var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("send logs", [])));
        var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
        lock (_lock)
        {
            _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public void RequestStats()
    {
        var bin = Encoding.UTF8.GetBytes(FormatMessage(new WsMessage("send stats", [])));
        var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
        lock (_lock)
        {
            _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private string FormatMessage(WsMessage message)
    {
        return JsonSerializer.Serialize(message, _jsonOptions);
    }

    /// <summary>
    ///     Connects the websocket to the server.
    /// </summary>
    /// <param name="url">The url to the server.</param>
    /// <param name="token">The token you got from <see cref="IApiService.GetWebsocketAsync"/></param>
    public void Connect(Uri url, string token)
    {
        if (_ws.State is WebSocketState.Aborted or WebSocketState.Closed)
        {
            _ws.Dispose();
            _ws = new ClientWebSocket();
        }

        _ws.ConnectAsync(url, CancellationToken.None).GetAwaiter().GetResult();
        Auth(token);
    }

    /// <summary>
    ///     Connects the websocket to the server. This is the asynchronous version of <see cref="Connect" />.
    /// </summary>
    /// <param name="url">Url to the server.</param>
    /// <param name="token">The token you got from <see cref="IApiService.GetWebsocketAsync"/></param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ConnectAsync(Uri url, string token, CancellationToken cancellationToken)
    {
        if (_ws.State is WebSocketState.Aborted or WebSocketState.Closed)
        {
            _ws.Dispose();
            _ws = new ClientWebSocket();
        }

        await _ws.ConnectAsync(url, cancellationToken);
        Auth(token);
    }

    /// <summary>
    ///     Closes the websocket connection. The <see cref="ApiWsClient" /> is still reusable after closure.
    /// </summary>
    public void Close()
    {
        if (_ws.State == WebSocketState.Open)
            _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    /// <summary>
    ///     Closes the websocket connection. This is the asynchronous version of <see cref="Close" /> The
    ///     <see cref="ApiWsClient" /> is still reusable after closure.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        if (_ws.State == WebSocketState.Open)
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
    }

    /// <summary>
    ///     Listens to the websocket and invokes any actions that are needed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        while (_ws.State == WebSocketState.Open || cancellationToken.IsCancellationRequested)
        {
            WebSocketReceiveResult result;

            using var ms = new MemoryStream();
            do
            {
                var buffer = WebSocket.CreateClientBuffer(1024, 16);
                result = await _ws.ReceiveAsync(buffer, cancellationToken);
                if (buffer.Array != null) ms.Write(buffer.Array, 0, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close) break;
            if (result.MessageType != WebSocketMessageType.Text) continue;

            var msg = Encoding.UTF8.GetString(ms.ToArray());
            if (string.IsNullOrEmpty(msg)) continue;
            var message = JsonSerializer.Deserialize<WsMessage>(msg, _jsonOptions);
            if (message != null)
                switch (message.Event)
                {
                    case "auth success":
                        AuthSuccess?.Invoke();
                        break;
                    case "backup complete":
                        BackupComplete?.Invoke();
                        break;
                    case "backup restore completed":
                        BackupRestoreCompleted?.Invoke();
                        break;
                    case "console output":
                        ConsoleOutput?.Invoke(message.Args[0]);
                        break;
                    case "daemon error":
                        DaemonError?.Invoke(message.Args[0]);
                        break;
                    case "daemon message":
                        DaemonMessage?.Invoke(message.Args[0]);
                        break;
                    case "install completed":
                        InstallCompleted?.Invoke();
                        break;
                    case "install output":
                        InstallOutput?.Invoke(message.Args[0]);
                        break;
                    case "install started":
                        InstallStarted?.Invoke();
                        break;
                    case "jwt error":
                        JwtError?.Invoke(message.Args[0]);
                        break;
                    case "stats":
                    {
                        var deJson = JsonSerializer.Deserialize<StatsWsResponse>(message.Args[0], _jsonOptions);
                        if (deJson != null) Stats?.Invoke(deJson);
                    }
                        break;
                    case "status":
                        switch (message.Args[0])
                        {
                            case "starting":
                                Status?.Invoke(PowerStatus.Starting);
                                break;
                            case "stopping":
                                Status?.Invoke(PowerStatus.Stopping);
                                break;
                            case "offline":
                                Status?.Invoke(PowerStatus.Offline);
                                break;
                            case "online":
                                Status?.Invoke(PowerStatus.Online);
                                break;
                        }

                        break;
                    case "token expired":
                        TokenExpired?.Invoke();
                        break;
                    case "token expiring":
                        TokenExpiring?.Invoke();
                        break;
                    case "transfer logs":
                        TransferLogs?.Invoke(message.Args[0]);
                        break;
                    case "transfer status":
                        TransferStatus?.Invoke(message.Args[0]);
                        break;
                }
        }
    }

    private void ReleaseUnmanagedResources()
    {
        if (_ws.State is WebSocketState.Open or WebSocketState.CloseReceived or WebSocketState.CloseSent)
            _ws.Abort();
        else
            _ws.Dispose();
    }

    ~ApiWsClient()
    {
        ReleaseUnmanagedResources();
    }
}