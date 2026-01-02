using System.Collections.Concurrent;

// ReSharper disable InconsistentNaming

namespace PCL.Neo.Core.Utils.Net;

/// <summary>
/// 提供HTTP客户端池的静态类，防止多次创建HTTP客户端引起的性能消耗
/// </summary>
internal static class HttpClientPool
{
    private static readonly ConcurrentDictionary<string, HttpClientInfo> _clients = new();
    private static readonly Timer _cleanupTimer;
    private static readonly object _cleanupLock = new();

    static HttpClientPool()
    {
        // 每60秒检查一次过期的HttpClient
        _cleanupTimer = new Timer(CleanupExpiredClients, null, TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
    }

    internal class HttpClientInfo(HttpClient client, DateTime expirationTime, bool inUse)
    {
        public HttpClient Client { get; } = client;
        public DateTime ExpirationTime { get; } = expirationTime;

        public bool InUse { get; set; } = inUse;
    }

    /// <summary>
    /// 添加HTTP客户端到池中，可以指定自动销毁时间
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <param name="client">HTTP客户端实例</param>
    /// <param name="lifetime">客户端生命周期，默认为1小时</param>
    public static HttpClientInfo AddHttpClient(string name, HttpClient client, TimeSpan? lifetime = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(client);

        var expirationTime = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(5));
        var clientInfo = new HttpClientInfo(client, expirationTime, true);

        if (!_clients.TryAdd(name, clientInfo))
        {
            // 如果已存在同名客户端，先释放旧的再添加新的
            if (_clients.TryRemove(name, out var oldClientInfo))
            {
                oldClientInfo.Client.Dispose();
            }

            _clients.TryAdd(name, clientInfo);
        }

        return clientInfo;
    }

    /// <summary>
    /// 获取指定名称的HTTP客户端实例，如果不存在或已过期则创建一个新的实例
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <param name="lifetime">新建客户端的生命周期，默认为1小时</param>
    /// <returns>HTTP客户端实例</returns>
    public static HttpClientInfo GetClient(string name, TimeSpan? lifetime = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_clients.TryGetValue(name, out var clientInfo))
        {
            if (DateTime.UtcNow < clientInfo.ExpirationTime)
            {
                return clientInfo;
            }

            // 如果客户端已过期，移除并释放它
            if (_clients.TryRemove(name, out var expiredClientInfo))
            {
                expiredClientInfo.Client.Dispose();
            }
        }

        // 创建新的客户端
        var client = new HttpClient();
        var returnClientInfo = AddHttpClient(name, client, lifetime);
        return returnClientInfo;
    }

    /// <summary>
    /// 释放指定名称的HTTP客户端实例
    /// </summary>
    /// <param name="name">客户端名称</param>
    /// <returns>是否成功释放</returns>
    public static bool ReleaseClient(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (_clients.TryRemove(name, out var clientInfo))
        {
            clientInfo.InUse = false;
            clientInfo.Client.Dispose();
            return true;
        }

        return false;
    }

    /// <summary>
    /// 清理所有过期的HTTP客户端
    /// </summary>
    private static void CleanupExpiredClients(object? state)
    {
        // 使用锁防止多个清理操作同时进行
        if (!Monitor.TryEnter(_cleanupLock))
        {
            return;
        }

        try
        {
            var now = DateTime.UtcNow;
            var expiredClients = _clients.Where(kvp => now >= kvp.Value.ExpirationTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var name in expiredClients)
            {
                if (_clients.TryRemove(name, out var clientInfo))
                {
                    if (clientInfo.InUse)
                    {
                        continue;
                    }

                    clientInfo.Client.Dispose();
                }
            }
        }
        finally
        {
            Monitor.Exit(_cleanupLock);
        }
    }
}
