namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

internal sealed class Unsubscriber<TRedirectAuthCode> : IDisposable
{
    private readonly IObserver<TRedirectAuthCode> _observer;
    private readonly ISet<IObserver<TRedirectAuthCode>> _observers;

    internal Unsubscriber(ISet<IObserver<TRedirectAuthCode>> observers, IObserver<TRedirectAuthCode> observer)
    {
        (_observers, _observer) = (observers, observer);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _observers.Remove(_observer);
    }
}
