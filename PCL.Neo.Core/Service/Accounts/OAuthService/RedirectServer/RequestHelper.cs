using System.Net;

namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

public class RequestHelper(HttpListenerRequest request)
{
    public delegate void ExecutingDespatch(FileStream fileStream);

    private HttpListenerRequest Request { get; } = request;
    public Stream RequestStream { get; set; } = request.InputStream;

    public void DispatchResources(ExecutingDespatch action, out RedirectAuthCode authCode)
    {
        var code = Request.QueryString["code"];
        ArgumentNullException.ThrowIfNull(code);
        authCode = new RedirectAuthCode(code);

        var file = new FileStream("OAuthRedirectHttpPage.html", FileMode.Open, FileAccess.Read);

        action?.Invoke(file);
    }
}
