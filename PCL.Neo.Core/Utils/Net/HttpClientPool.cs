// ReSharper disable InconsistentNaming

namespace PCL.Neo.Core.Utils.Net;

/// <summary>
/// 提供HTTP客户端池的静态类，防止多次创建HTTP客户端引起的性能消耗
/// </summary>
internal static class HttpClientPool
{
    private static readonly Dictionary<string, HttpClient> _clients = new();
    private static readonly Lock _lock = new();

    public static void AddHttpClient(string name, HttpClient client)
    {
        lock (_lock)
        {
            _clients.Add(name, client);
        }
    }

    /// <summary>
    /// 获取指定名称的HTTP客户端实例，如果不存在则创建一个新的实例
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static HttpClient GetClient(string name)
    {
        lock (_lock)
        {
            if (!_clients.TryGetValue(name, out var client))
            {
                client = new HttpClient();
                _clients[name] = client;
            }

            return client;
        }
    }

    /// <summary>
    /// 释放指定名称的HTTP客户端实例
    /// </summary>
    /// <param name="name"></param>
    public static void ReleaseClient(string name)
    {
        lock (_lock)
        {
            if (_clients.ContainsKey(name))
            {
                _clients[name].Dispose();
                _clients.Remove(name);
            }
        }
    }
}
