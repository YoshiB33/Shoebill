using System;
using System.Collections.Concurrent;
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
        throw new NotImplementedException();
    }

    public void SendCommand(string command)
    {
        throw new NotImplementedException();
    }

    public void RequestLogs()
    {
        throw new NotImplementedException();
    }

    public void RequestStats()
    {
        throw new NotImplementedException();
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


    public void Connect(Uri url, string token)
    {
        _ws.ConnectAsync(url, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task ConnectAsync(Uri url, string token, CancellationToken cancellationToken)
    {
        await _ws.ConnectAsync(url, cancellationToken);
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
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