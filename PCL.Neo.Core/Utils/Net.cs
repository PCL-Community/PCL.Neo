using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PCL.Neo.Core.Utils;

#pragma warning disable IL2026 // will fixed by dynamic dependency
public static class Net
{
    public static IFlurlClient FlurlClient { get; private set; }

    public static IFlurlRequest Request(Url url)
    {
        if (FlurlClient is null)
            throw new InvalidOperationException("FlurlClient is not initialized.");

        return FlurlClient.Request(url);
    }

    public static IFlurlRequest Request(string url)
    {
        if (FlurlClient is null)
            throw new InvalidOperationException("FlurlClient is not initialized.");

        return FlurlClient.Request(url);
    }
    public static IFlurlClient Initialize(IFlurlClient client = default)
    {
        if (client is not null)
            return FlurlClient = client;

        return FlurlClient = new FlurlClient
        {
            Settings = {
                Timeout = TimeSpan.FromSeconds(100),
                JsonSerializer = new DefaultJsonSerializer(),
            },
            Headers = {
                { "User-Agent", "PCL.Neo.Net/1.0" },
            },
        };
    }
    /// <summary>
    /// 疑似有点小问题，尽量别用
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="method">Post/Get</param>
    /// <param name="url">请求的Url</param>
    /// <param name="content">请求内容</param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public static async Task<TResponse> SendHttpRequestAsync<TResponse>(
        HttpMethod method,
        Uri url,
        object? content = null,
        string? bearerToken = null)
    {
        using var request = new HttpRequestMessage(method, url);

        // 设置请求体
        if (content != null)
        {
            if (content is FormUrlEncodedContent formContent)
            {
                request.Content = formContent;
            }
            else
            {
                request.Content = JsonContent.Create(content);
            }
        }

        // 设置授权头
        if (!string.IsNullOrEmpty(bearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        // 发送请求
        using var response = await Shared.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        // 解析响应
        var result = await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(result);

        return result;
    }
}