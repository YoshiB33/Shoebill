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

    public ApiWsClient()
    {
        _ws = new ClientWebSocket();
    }

    public event Action AuthSuccess;

    public event Action BackupComplete;

    public event Action BackupRestoreCompleted;

    public event Action<string> ConsoleOutput;

    public event Action<string> DaemonError;

    public event Action<string> DaemonMessage;

    public event Action InstallCompleted;

    public event Action<string> InstallOutput;

    public event Action InstallStarted;

    public event Action<string> JwtError;

    public event Action<string> Stats;

    public event Action<PowerStatus> Status;

    public event Action TokenExpired;

    public event Action TokenExpiring;

    public event Action<string> TransferLogs;

    public event Action<string> TransferStatus;

    public void Auth(string token)
    {
        throw new NotImplementedException();
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