namespace PCL.Neo.Core.Utils.Net;

public interface INetService
{
    Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string url);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);

    Task<T> SendAsync<T>(HttpMethod httpMethod, string url);
    Task<T> SendAsync<T>(HttpRequestMessage request);

    Task<HttpResponseMessage> SendJsonAsync<TJson>(HttpMethod httpMethod, string url, TJson content);
    Task<TResult> SendJsonAsync<TResult, TJson>(HttpMethod httpMethod, string url, TJson content);
}
