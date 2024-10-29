using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Shoebill.Services;

/// <summary>
///     An object that is able to communicate with the pterodactyl websocket.
/// </summary>
/// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md" />
public class ApiWsClient : IApiWsClient, IDisposable
{
    private readonly ClientWebSocket _ws;

    /// <summary>
    ///     Creates the websocket client.
    /// </summary>
    public ApiWsClient()
    {
        _ws = new ClientWebSocket();
    }

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>auth success</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action AuthSuccess;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>backup complete</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action BackupComplete;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>backup restore completed</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action BackupRestoreCompleted;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>console output</c> event where the <see cref="string" /> is
    ///     the one line of console output received.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> ConsoleOutput;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>daemon error</c> event where the <see cref="string" /> is
    ///     the error message received.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> DaemonError;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>daemon message</c> event where the <see cref="string" /> is
    ///     the message received.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> DaemonMessage;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>install completed</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action InstallCompleted;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>install output</c> event where the <see cref="string" /> is
    ///     the output message.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> InstallOutput;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>install started</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action InstallStarted;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>jwt error</c> event where the <see cref="string" /> is the
    ///     error message.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> JwtError;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>stats</c> event where the <see cref="string" /> is the
    ///     current stats in json format.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> Stats;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>status</c> event where the <see cref="PowerStatus" /> is the
    ///     power state of the server.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<PowerStatus> Status;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>token expired</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action TokenExpired;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>token expiring</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action TokenExpiring;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>token expired</c> event.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> TransferLogs;

    /// <summary>
    ///     An event for knowing when the websocket receive the <c>transfer status</c> event where the <see cref="string" /> is
    ///     the transfer status.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-recieve" />
    public event Action<string> TransferStatus;

    /// <summary>
    ///     Authenticates the websocket connection.
    /// </summary>
    /// <param name="token">The websocket auth token that you got from <see cref="IApiService.GetWebsocketAsync" /></param>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void Auth(string token)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sets the power state of the server.
    /// </summary>
    /// <param name="action">The power action you want to set.</param>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void SetState(PowerAction action)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Sends a command to the server console.
    /// </summary>
    /// <param name="command">The command you send.</param>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void SendCommand(string command)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Requests the console logs for the server.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void RequestLogs()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Requests the stats for the server.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void RequestStats()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }


    public void Connect(Uri url, string token)
    {
        _ws.ConnectAsync(url, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task ConnectAsync(Uri url, string token, CancellationToken cancellationToken)
    {
        await _ws.ConnectAsync(url, cancellationToken);
    }

    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    ~ApiWsClient()
    {
        ReleaseUnmanagedResources();
    }
}