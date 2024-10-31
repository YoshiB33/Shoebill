using System;

namespace Shoebill.Services;

public enum PowerStatus
{
    Starting,
    Stopping,
    Online,
    Offline
}

public enum PowerAction
{
    Start,
    Stop,
    Restart,
    Kill
}

/// <summary>
///     An interface that contains all properties and functions required to communicate with the pterodactyl websocket.
/// </summary>
/// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md" />
public interface IApiWsClient
{
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
    /// <exception cref="ArgumentNullException">The token parameter is null</exception>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void Auth(string token);

    /// <summary>
    ///     Sets the power state of the server.
    /// </summary>
    /// <param name="action">The power action you want to set.</param>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    /// <exception cref="ArgumentOutOfRangeException">The action parameter is not valid</exception>
    public void SetState(PowerAction action);

    /// <summary>
    ///     Sends a command to the server console.
    /// </summary>
    /// <param name="command">The command you send.</param>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void SendCommand(string command);

    /// <summary>
    ///     Requests the console logs for the server.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void RequestLogs();

    /// <summary>
    ///     Requests the stats for the server.
    /// </summary>
    /// <seealso href="https://github.com/devnote-dev/ptero-notes/blob/main/wings/websocket.md#events-you-can-send" />
    public void RequestStats();
}