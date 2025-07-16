using PCL.Neo.Core.Utils.Logger;
using System.Text;
using System.Text.Json;

namespace PCL.Neo.Core.Utils.Net;

/// <summary>
/// HTTP客户端服务实现
/// </summary>
public static class StaticNet
{
    private static readonly string SharedName;

    static StaticNet()
    {
        var sharedName = Guid.NewGuid().ToString()[..8];
        var clientInfo = HttpClientPool.AddHttpClient(sharedName, new HttpClient(), TimeSpan.FromMinutes(5));
        clientInfo.InUse = false;
        SharedName = sharedName;
    }

    /// <summary>
    /// 发送无内容信息
    /// </summary>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="token">令牌</param>
    /// <returns>HTTP响应消息</returns>
    public static async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, Uri url, string? token = null)
    {
        var strUrl = url.ToString();
        LogRequest(httpMethod, strUrl, "无内容");

        try
        {
            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var request = new HttpRequestMessage(httpMethod, url);
            var response = await httpClient.SendAsync(request);

            LogResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, strUrl, ex);
            throw;
        }
    }

    /// <summary>
    /// 发送信息
    /// </summary>
    /// <param name="request">请求内容</param>
    /// <param name="token">令牌</param>
    /// <returns>HTTP响应消息</returns>
    public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, string? token = null)
    {
        var httpMethod = request.Method;
        var url = request.RequestUri?.ToString() ?? string.Empty;
        LogRequest(httpMethod, url, nameof(request));

        try
        {
            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.SendAsync(request);

            LogResponse(response);
            return response;
        }
        catch (Exception e)
        {
            LogError(httpMethod, url, e);
            throw;
        }
    }

    /// <summary>
    /// 发送无内容信息，返回JSON反序列化对象
    /// </summary>
    /// <typeparam name="T">返回对象类型</typeparam>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="token">令牌</param>
    /// <returns>反序列化后的对象</returns>
    public static async Task<T> SendAsync<T>(HttpMethod httpMethod, Uri url, string? token = null)
    {
        var strUrl = url.ToString();
        LogRequest(httpMethod, strUrl, "无内容", typeof(T));

        try
        {
            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var request = new HttpRequestMessage(httpMethod, url);
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            LogResponse(response);

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

#nullable disable
            var result = JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, strUrl, ex);
            throw;
        }
#nullable restore
    }

    /// <summary>
    /// 发送信息
    /// </summary>
    /// <param name="request">请求内容</param>
    /// <param name="token">令牌</param>
    /// <returns>HTTP响应消息</returns>
    public static async Task<T> SendAsync<T>(HttpRequestMessage request, string? token = null)
    {
        var method = request.Method;
        var url = request.RequestUri?.ToString() ?? string.Empty;
        LogRequest(method, url, nameof(request), typeof(T));

        try
        {
            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            LogResponse(response);

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

#nullable disable
            var result = JsonSerializer.Deserialize<T>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        catch (Exception ex)
        {
            LogError(method, url, ex);
            throw;
        }
#nullable restore
    }

    /// <summary>
    /// 发送JSON内容
    /// </summary>
    /// <typeparam name="TJson">发送的Json数据类型</typeparam>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="token">令牌</param>
    /// <param name="content">要发送的内容对象</param>
    /// <returns>HTTP响应消息</returns>
    public static async Task<HttpResponseMessage> SendJsonAsync<TJson>(HttpMethod httpMethod, Uri url, TJson content,
        string? token = null)
    {
        var strUrl = url.ToString();

        LogRequest(httpMethod, strUrl, "JSON内容", typeof(TJson));

        try
        {
            var request = new HttpRequestMessage(httpMethod, url);

            if (content != null)
            {
                var jsonContent = JsonSerializer.Serialize(content);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.SendAsync(request);

            LogResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, strUrl, ex);
            throw;
        }
    }

    /// <summary>
    /// 发送JSON内容，返回JSON反序列化对象
    /// </summary>
    /// <typeparam name="TResult">返回对象类型</typeparam>
    /// <typeparam name="TJson">发送的Json数据类型</typeparam>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="token">令牌</param>
    /// <param name="content">要发送的内容对象</param>
    /// <returns>反序列化后的对象</returns>
    public static async Task<TResult> SendJsonAsync<TResult, TJson>(HttpMethod httpMethod, Uri url, TJson content,
        string? token = null)
    {
        var strUrl = url.ToString();

        LogRequest(httpMethod, strUrl, "JSON内容", content?.GetType(), typeof(TResult));

        try
        {
            var request = new HttpRequestMessage(httpMethod, url);

            var jsonContent = JsonSerializer.Serialize(content);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = GetHttpClient(ref token);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            var response = await httpClient.SendAsync(request);

            LogResponse(response);

            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
#nullable disable
            var result = JsonSerializer.Deserialize<TResult>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, strUrl, ex);
            throw;
        }
#nullable restore
    }

    /// <summary>
    /// 记录请求日志
    /// </summary>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="contentType">内容类型描述</param>
    /// <param name="requestType">请求对象类型</param>
    /// <param name="responseType">响应对象类型</param>
    private static void LogRequest(
        HttpMethod httpMethod,
        string url,
        string contentType,
        Type? requestType = null,
        Type? responseType = null)
    {
        var logMessage = $"HTTP请求 - " +
                         $"方法: {httpMethod} | " +
                         $"URL: {url} | " +
                         $"内容类型: {contentType}";

        if (requestType != null)
            logMessage += $" | 请求类型: {requestType.Name}";

        if (responseType != null)
            logMessage += $" | 响应类型: {responseType.Name}";

        NewLogger.Logger.LogInformation(logMessage);
    }

    /// <summary>
    /// 记录响应日志
    /// </summary>
    /// <param name="response">HTTP响应</param>
    private static void LogResponse(HttpResponseMessage response)
    {
        var logMessage = $"HTTP响应 - " +
                         $"状态码: {(int)response.StatusCode} {response.StatusCode} | " +
                         $"内容类型: {response.Content?.Headers?.ContentType?.MediaType ?? "未知"}";

        NewLogger.Logger.LogInformation(logMessage);
    }

    /// <summary>
    /// 记录错误日志
    /// </summary>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="exception">异常信息</param>
    private static void LogError(HttpMethod httpMethod, string url, Exception exception)
    {
        var logMessage = $"HTTP错误 - " +
                         $"方法: {httpMethod} | " +
                         $"URL: {url} | " +
                         $"错误: {exception.Message}";

        NewLogger.Logger.LogError(logMessage);
    }

    private static HttpClient GetHttpClient(ref string? token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return new HttpClient();
        }
        else
        {
            var clientInfo = HttpClientPool.GetClient(SharedName);
            clientInfo.InUse = false;
            return clientInfo.Client;
        }
    }
}
