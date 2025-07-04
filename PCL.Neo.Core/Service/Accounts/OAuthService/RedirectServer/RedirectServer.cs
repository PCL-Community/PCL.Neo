using System.Net;

namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

public class RedirectServer : IObservable<RedirectAuthCode>
{
    private readonly HashSet<IObserver<RedirectAuthCode>> _observers = [];
    private RedirectAuthCode? _authCode;

    public RedirectServer(ushort port, bool startImmediately = true)
    {
        Port = port;
        if (startImmediately)
        {
            Start();
        }
    }

    private ushort Port { get; }
    private HttpListener Listener { get; } = new();

    /// <inheritdoc />
    public IDisposable Subscribe(IObserver<RedirectAuthCode> observer)
    {
        if (!_observers.Add(observer))
        {
            return new Unsubscriber<RedirectAuthCode>(_observers, observer);
        }

        if (_authCode is not null)
        {
            observer.OnNext(_authCode);
        }

        return new Unsubscriber<RedirectAuthCode>(_observers, observer);
    }

    /// <summary>
    /// 开始监听重定向请求
    /// </summary>
    public void StartListening()
    {
        Start();
    }

    private void Start()
    {
        Listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        Listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
        Listener.Start();

        Receive();
    }

    public void Close()
    {
        Finished();
    }

    private void Receive()
    {
        Listener.BeginGetContext(EndReceive, null);
    }

    private void EndReceive(IAsyncResult asyncResult)
    {
        var context = Listener.EndGetContext(asyncResult);
        Dispather(context);
        Receive();
    }

    private void Dispather(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        var requestHelper = new RequestHelper(request);
        var reponseHelper = new ResponseHelper(response);

        requestHelper.DispatchResources(fileStream => reponseHelper.WriteContent(fileStream),
            out var authCode);
        _authCode = authCode;

        Notify();
        Finished();
    }

    private void Notify()
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(_authCode!);
        }
    }

    private void Finished()
    {
        foreach (var observer in _observers)
        {
            observer.OnCompleted();
        }

        Listener.Stop();
    }
}
