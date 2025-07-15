using PCL.Neo.Core.Utils.Logger;
using System.Text;
using System.Text.Json;

namespace PCL.Neo.Core.Utils.Net;

/// <summary>
/// HTTP客户端服务实现
/// </summary>
public class NetService : INetService
{
    private readonly string _token;
    private readonly string _httpClientName;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="httpClientName">HTTP客户端名称</param>
    /// <param name="baseUrl">基础URL（可选）</param>
    /// <param name="token">令牌（可选）</param>
    public NetService(
        string httpClientName,
        string? baseUrl = null,
        string? token = null)
    {
        _httpClientName = httpClientName ?? throw new ArgumentNullException(nameof(httpClientName));
        var baseUrl1 = baseUrl?.TrimEnd('/') ?? string.Empty;
        _token = token ?? string.Empty;
        HttpClientPool.AddHttpClient(_httpClientName, new HttpClient
        {
            BaseAddress = new Uri(baseUrl1)
        });
    }

    /// <summary>
    /// 发送无内容信息
    /// </summary>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <returns>HTTP响应消息</returns>
    public async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod, string url)
    {
        var httpClient = CreateHttpClient();

        LogRequest(httpMethod, url, "无内容");

        try
        {
            var request = new HttpRequestMessage(httpMethod, url);
            var response = await httpClient.SendAsync(request);

            LogResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, url, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        var httpMethod = request.Method;
        var url = request.RequestUri?.ToString() ?? string.Empty;
        var httpClient = CreateHttpClient();

        LogRequest(httpMethod, url, nameof(request));

        try
        {
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
    /// <returns>反序列化后的对象</returns>
    public async Task<T> SendAsync<T>(HttpMethod httpMethod, string url)
    {
        var httpClient = CreateHttpClient();

        LogRequest(httpMethod, url, "无内容", typeof(T));

        try
        {
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
            LogError(httpMethod, url, ex);
            throw;
        }
#nullable restore
    }

    /// <inheritdoc />
    public async Task<T> SendAsync<T>(HttpRequestMessage request)
    {
        var httpClient = CreateHttpClient();
        var method = request.Method;
        var url = request.RequestUri?.ToString() ?? string.Empty;
        LogRequest(method, url, nameof(request), typeof(T));

        try
        {
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
    /// <param name="content">要发送的内容对象</param>
    /// <returns>HTTP响应消息</returns>
    public async Task<HttpResponseMessage> SendJsonAsync<TJson>(HttpMethod httpMethod, string url, TJson content)
    {
        var httpClient = CreateHttpClient();

        LogRequest(httpMethod, url, "JSON内容", typeof(TJson));

        try
        {
            var request = new HttpRequestMessage(httpMethod, url);

            if (content != null)
            {
                var jsonContent = JsonSerializer.Serialize(content);
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var response = await httpClient.SendAsync(request);

            LogResponse(response);
            return response;
        }
        catch (Exception ex)
        {
            LogError(httpMethod, url, ex);
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
    /// <param name="content">要发送的内容对象</param>
    /// <returns>反序列化后的对象</returns>
    public async Task<TResult> SendJsonAsync<TResult, TJson>(HttpMethod httpMethod, string url, TJson content)
    {
        var httpClient = CreateHttpClient();

        LogRequest(httpMethod, url, "JSON内容", content?.GetType(), typeof(TResult));

        try
        {
            var request = new HttpRequestMessage(httpMethod, url);

            var jsonContent = JsonSerializer.Serialize(content);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

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
            LogError(httpMethod, url, ex);
            throw;
        }
#nullable restore
    }

    /// <summary>
    /// 创建HTTP客户端
    /// </summary>
    /// <returns>配置好的HTTP客户端</returns>
    private HttpClient CreateHttpClient()
    {
        var httpClient = HttpClientPool.GetClient(_httpClientName);
        // 如果有令牌，添加到请求头
        if (!string.IsNullOrEmpty(_token))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
        }

        return httpClient;
    }

    /// <summary>
    /// 记录请求日志
    /// </summary>
    /// <param name="httpMethod">HTTP方法</param>
    /// <param name="url">目标URL</param>
    /// <param name="contentType">内容类型描述</param>
    /// <param name="requestType">请求对象类型</param>
    /// <param name="responseType">响应对象类型</param>
    private void LogRequest(
        HttpMethod httpMethod,
        string url,
        string contentType,
        Type? requestType = null,
        Type? responseType = null)
    {
        var logMessage = $"HTTP请求 - " +
                         $"客户端: {_httpClientName} | " +
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
    private void LogResponse(HttpResponseMessage response)
    {
        var logMessage = $"HTTP响应 - " +
                         $"客户端: {_httpClientName} | " +
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
    private void LogError(HttpMethod httpMethod, string url, Exception exception)
    {
        var logMessage = $"HTTP错误 - " +
                         $"客户端: {_httpClientName} | " +
                         $"方法: {httpMethod} | " +
                         $"URL: {url} | " +
                         $"错误: {exception.Message}";

        NewLogger.Logger.LogError(logMessage);
    }
}
