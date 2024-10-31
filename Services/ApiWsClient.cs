using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Shoebill.Models.Api;

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

    private readonly BlockingCollection<WsMessage> _queue = new();
    private readonly ClientWebSocket _ws;

    public ApiWsClient()
    {
        _ws = new ClientWebSocket();

        var thread = new Thread(Queue)
        {
            IsBackground = true
        };
        thread.Start();
    }

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

    public event Action<string>? Stats;

    public event Action<PowerStatus>? Status;

    public event Action? TokenExpired;

    public event Action? TokenExpiring;

    public event Action<string>? TransferLogs;

    public event Action<string>? TransferStatus;

    public void Auth(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(nameof(token));

        Enqueue(new WsMessage("auth", [token]));
    }

    public void SetState(PowerAction action)
    {
        switch (action)
        {
            case PowerAction.Start:
                Enqueue(new WsMessage("set state", ["start"]));
                break;
            case PowerAction.Stop:
                Enqueue(new WsMessage("set state", ["stop"]));
                break;
            case PowerAction.Restart:
                Enqueue(new WsMessage("set state", ["restart"]));
                break;
            case PowerAction.Kill:
                Enqueue(new WsMessage("set state", ["kill"]));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void SendCommand(string command)
    {
        Enqueue(new WsMessage("send command", [command]));
    }

    public void RequestLogs()
    {
        Enqueue(new WsMessage("send logs", []));
    }

    public void RequestStats()
    {
        Enqueue(new WsMessage("send stats", []));
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

    private void Queue()
    {
        while (!_queue.IsCompleted)
            foreach (var message in _queue)
            {
                var bin = Encoding.UTF8.GetBytes(FormatMessage(message));
                var buffer = new ArraySegment<byte>(bin, 0, bin.Length);
                _ws.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
    }

    private void Enqueue(WsMessage message)
    {
        _queue.Add(message);
    }

    /// <summary>
    ///     Connects the websocket to the server.
    /// </summary>
    /// <param name="url">The url to the server.</param>
    public void Connect(Uri url)
    {
        _ws.ConnectAsync(url, CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Connects the websocket to the server. This is the asynchronous version of <see cref="Connect" />.
    /// </summary>
    /// <param name="url">Url to the server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ConnectAsync(Uri url, CancellationToken cancellationToken)
    {
        await _ws.ConnectAsync(url, cancellationToken);
    }

    /// <summary>
    ///     Closes the websocket connection. The <see cref="ApiWsClient" /> is still reusable after closure.
    /// </summary>
    public void Close()
    {
        _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
    }

    /// <summary>
    ///     Closes the websocket connection. This is the asynchronous version of <see cref="Close" /> The
    ///     <see cref="ApiWsClient" /> is still reusable after closure.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
    }

    /// <summary>
    ///     Listens to the websocket and invokes any actions that are needed.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task ReceiveAsync(CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        while (_ws.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;

            do
            {
                var buffer = WebSocket.CreateClientBuffer(1024, 16);
                result = await _ws.ReceiveAsync(buffer, cancellationToken);
            } while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text) continue;

            var msg = Encoding.UTF8.GetString(ms.ToArray());
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
                        Stats?.Invoke(message.Args[0]);
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
        _queue.CompleteAdding();
        _queue.Dispose();
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