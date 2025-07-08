using PCL.Neo.Core.Service.Accounts.Exceptions;
using PCL.Neo.Core.Service.Accounts.IYggdrasilAuth.Data;
using PCL.Neo.Core.Service.Accounts.Storage;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.IYggdrasilAuth;

/// <summary>
/// Yggdrasil外置登录认证服务的实现
/// </summary>
public class YggdrasilAuthService : IYggdrasilAuthService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public async Task<YggdrasilAccount> AuthenticateAsync(
        string serverUrl,
        string username,
        string password,
        int timeoutMs = 10000)
    {
        if (string.IsNullOrEmpty(serverUrl))
            throw new ArgumentException("服务器URL不能为空", nameof(serverUrl));

        if (string.IsNullOrEmpty(username))
            throw new ArgumentException("用户名不能为空", nameof(username));

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        // 格式化服务器URL，去除末尾的斜杠
        var baseUrl = serverUrl.TrimEnd('/');
        var authEndpoint = $"{baseUrl}/authserver/authenticate";

        this.LogYggdrasilInfo($"开始外置登录认证: {username} @ {serverUrl}");

        // 生成客户端令牌
        var clientToken = Guid.NewGuid().ToString();

        // 构建认证请求
        var authRequest = new YggdrasilAuthRequest
        {
            Agent = new YggdrasilAgent { Name = "Minecraft", Version = 1 },
            Username = username,
            Password = password,
            ClientToken = clientToken,
            RequestUser = true
        };

        try
        {
            // 配置超时时间
            Shared.HttpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);


            // 序列化请求体
            var requestJson = JsonSerializer.Serialize(authRequest, _jsonOptions);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // 发送请求
            var response = await Shared.HttpClient.PostAsync(authEndpoint, content).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // 尝试解析错误响应
                YggdrasilErrorResponse? errorResponse = null;
                try
                {
                    errorResponse = JsonSerializer.Deserialize<YggdrasilErrorResponse>(responseContent, _jsonOptions);
                }
                catch
                {
                    // 忽略解析错误
                }

                var errorType = errorResponse?.Error ?? "unknown";
                var errorMessage = errorResponse?.ErrorMessage ?? "未知错误";

                this.LogYggdrasilError($"外置登录认证失败: {response.StatusCode} - {errorType}: {errorMessage}");

                throw errorType.ToLower() switch
                {
                    "forbidden" or "unauthorized" or "unsupportedmediatype" => new YggdrasilAuthException(
                        "服务器拒绝了认证请求", serverUrl, errorType, errorMessage) { StatusCode = response.StatusCode },

                    "methodnotallowed" => new YggdrasilAuthException("服务器不接受当前请求方法", serverUrl, errorType,
                        errorMessage) { StatusCode = response.StatusCode },

                    "illegalargument" => new YggdrasilAuthException("提供了无效的参数", serverUrl, errorType, errorMessage)
                    {
                        StatusCode = response.StatusCode
                    },

                    "invalidcredentials" => new YggdrasilAuthException("用户名或密码错误", serverUrl, errorType,
                        errorMessage) { StatusCode = response.StatusCode },

                    "usernotfound" => new YggdrasilAuthException("找不到指定的用户", serverUrl, errorType, errorMessage)
                    {
                        StatusCode = response.StatusCode
                    },

                    _ => new YggdrasilAuthException($"认证失败: {response.StatusCode} - {errorType}: {errorMessage}",
                        serverUrl, errorType, errorMessage) { StatusCode = response.StatusCode }
                };
            }

            // 解析成功响应
            var authResponse = JsonSerializer.Deserialize<YggdrasilAuthResponse>(responseContent, _jsonOptions);

            if (authResponse == null)
            {
                this.LogYggdrasilError($"外置登录认证响应解析失败: {responseContent}");
                throw new YggdrasilAuthException("无法解析认证响应", serverUrl, "ParseError", responseContent);
            }

            if (authResponse.SelectedProfile == null)
            {
                this.LogYggdrasilError("外置登录认证成功但没有选择的角色");
                throw new YggdrasilAuthException("没有可用的游戏角色", serverUrl, "NoProfile",
                    "Authentication successful but no game profile was selected");
            }

            this.LogYggdrasilInfo($"外置登录认证成功: {authResponse.SelectedProfile.Name}");

            // 构建账户对象
            var account = new YggdrasilAccount
            {
                Uuid = authResponse.SelectedProfile.Id,
                UserName = authResponse.SelectedProfile.Name,
                McAccessToken = authResponse.AccessToken,
                ClientToken = authResponse.ClientToken,
                ServerUrl = baseUrl,
                ServerName = GetServerName(baseUrl),
                ExpiresAt = DateTime.UtcNow.AddHours(12), // Yggdrasil通常没有明确的过期时间，使用12小时作为默认值
                AddedTime = DateTime.Now,
                LastUsed = DateTime.Now,
                Skins = [], // 初始化为空列表
                Capes = [], // 初始化为空列表
                UserProperties = GetUserProperties(authResponse.User?.Properties)
            };

            // TODO: 解析皮肤和披风信息
            // 外置登录的皮肤信息处理暂时简化，未来可以扩展

            return account;
        }
        catch (TaskCanceledException)
        {
            this.LogYggdrasilError($"外置登录认证请求超时: {serverUrl}");
            throw new YggdrasilAuthException("认证请求超时", serverUrl, "Timeout", "Request timed out")
            {
                StatusCode = HttpStatusCode.RequestTimeout
            };
        }
        catch (HttpRequestException ex)
        {
            this.LogYggdrasilError($"外置登录认证请求异常: {ex.Message}", ex);
            throw new YggdrasilAuthException("无法连接到认证服务器", serverUrl, "ConnectionError", ex.Message)
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex) when (!(ex is YggdrasilAuthException))
        {
            this.LogYggdrasilError($"外置登录认证过程中发生异常: {ex.Message}", ex);
            throw new YggdrasilAuthException("认证过程中发生异常", serverUrl, "UnknownError", ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<YggdrasilAccount> RefreshAsync(YggdrasilAccount account, int timeoutMs = 10000)
    {
        if (account == null)
            throw new ArgumentNullException(nameof(account));

        if (string.IsNullOrEmpty(account.ServerUrl))
            throw new ArgumentException("账户没有服务器URL信息", nameof(account));

        if (string.IsNullOrEmpty(account.McAccessToken))
            throw new ArgumentException("账户没有访问令牌", nameof(account));

        if (string.IsNullOrEmpty(account.ClientToken))
            throw new ArgumentException("账户没有客户端令牌", nameof(account));

        var baseUrl = account.ServerUrl;
        var refreshEndpoint = $"{baseUrl}/authserver/refresh";

        this.LogYggdrasilInfo($"开始刷新外置登录令牌: {account.UserName} @ {account.ServerUrl}");

        // 构建刷新请求
        var refreshRequest = new YggdrasilRefreshRequest
        {
            AccessToken = account.McAccessToken, ClientToken = account.ClientToken, RequestUser = true
        };

        try
        {
            // 配置超时时间
            Shared.HttpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            // 序列化请求体
            var requestJson = JsonSerializer.Serialize(refreshRequest, _jsonOptions);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // 发送请求
            var response = await Shared.HttpClient.PostAsync(refreshEndpoint, content).ConfigureAwait(false);
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                // 尝试解析错误响应
                YggdrasilErrorResponse? errorResponse = null;
                try
                {
                    errorResponse = JsonSerializer.Deserialize<YggdrasilErrorResponse>(responseContent, _jsonOptions);
                }
                catch
                {
                    // 忽略解析错误
                }

                var errorType = errorResponse?.Error ?? "unknown";
                var errorMessage = errorResponse?.ErrorMessage ?? "未知错误";

                this.LogYggdrasilError($"刷新外置登录令牌失败: {response.StatusCode} - {errorType}: {errorMessage}");

                if (errorType.ToLower() == "invalidtoken")
                {
                    throw new YggdrasilAuthException("无效的访问令牌，需要重新登录", account.ServerUrl, errorType, errorMessage)
                    {
                        StatusCode = response.StatusCode, RequireRelogin = true
                    };
                }

                throw new YggdrasilAuthException(
                    $"刷新令牌失败: {response.StatusCode} - {errorType}: {errorMessage}",
                    account.ServerUrl, errorType, errorMessage) { StatusCode = response.StatusCode };
            }

            // 解析成功响应
            var refreshResponse = JsonSerializer.Deserialize<YggdrasilAuthResponse>(responseContent, _jsonOptions);

            if (refreshResponse == null)
            {
                this.LogYggdrasilError($"刷新令牌响应解析失败: {responseContent}");
                throw new YggdrasilAuthException("无法解析刷新响应", account.ServerUrl, "ParseError", responseContent);
            }

            this.LogYggdrasilInfo($"外置登录令牌刷新成功: {account.UserName}");

            // 更新账户信息
            var userProperties = GetUserProperties(refreshResponse.User?.Properties);
            var updatedAccount = account with
            {
                McAccessToken = refreshResponse.AccessToken,
                ClientToken = refreshResponse.ClientToken,
                ExpiresAt = DateTime.UtcNow.AddHours(12), // 更新过期时间
                LastUsed = DateTime.Now,
                UserProperties = !string.IsNullOrEmpty(userProperties) ? userProperties : account.UserProperties
            };

            return updatedAccount;
        }
        catch (TaskCanceledException)
        {
            this.LogYggdrasilError($"刷新外置登录令牌请求超时: {account.ServerUrl}");
            throw new YggdrasilAuthException("刷新请求超时", account.ServerUrl, "Timeout", "Request timed out")
            {
                StatusCode = HttpStatusCode.RequestTimeout
            };
        }
        catch (HttpRequestException ex)
        {
            this.LogYggdrasilError($"刷新外置登录令牌请求异常: {ex.Message}", ex);
            throw new YggdrasilAuthException("无法连接到认证服务器", account.ServerUrl, "ConnectionError", ex.Message)
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
        }
        catch (Exception ex) when (!(ex is YggdrasilAuthException || ex is ArgumentException ||
                                     ex is ArgumentNullException))
        {
            this.LogYggdrasilError($"刷新外置登录令牌过程中发生异常: {ex.Message}", ex);
            throw new YggdrasilAuthException("刷新过程中发生异常", account.ServerUrl, "UnknownError", ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidateAsync(YggdrasilAccount account, int timeoutMs = 5000)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (string.IsNullOrEmpty(account.ServerUrl))
        {
            throw new ArgumentException("账户没有服务器URL信息", nameof(account));
        }

        if (string.IsNullOrEmpty(account.McAccessToken))
        {
            throw new ArgumentException("账户没有访问令牌", nameof(account));
        }

        if (string.IsNullOrEmpty(account.ClientToken))
        {
            throw new ArgumentException("账户没有客户端令牌", nameof(account));
        }

        var baseUrl = account.ServerUrl;
        var validateEndpoint = $"{baseUrl}/authserver/validate";

        this.LogYggdrasilDebug($"验证外置登录令牌: {account.UserName} @ {account.ServerUrl}");

        // 构建验证请求
        var validateRequest = new { accessToken = account.McAccessToken, clientToken = account.ClientToken };

        try
        {
            // 配置超时时间
            Shared.HttpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            // 序列化请求体
            var requestJson = JsonSerializer.Serialize(validateRequest, _jsonOptions);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // 发送请求
            var response = await Shared.HttpClient.PostAsync(validateEndpoint, content).ConfigureAwait(false);

            // 204 No Content表示验证成功
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                this.LogYggdrasilDebug($"外置登录令牌有效: {account.UserName}");
                return true;
            }

            // 其他状态表示验证失败
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            this.LogYggdrasilWarning($"外置登录令牌无效: {response.StatusCode} - {responseContent}");
            return false;
        }
        catch (Exception ex)
        {
            this.LogYggdrasilWarning($"验证外置登录令牌时发生异常: {ex.Message}");
            return false;
        }
    }

    /// <inheritdoc />
    public async Task SignOutAsync(YggdrasilAccount account, int timeoutMs = 5000)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (string.IsNullOrEmpty(account.ServerUrl))
        {
            throw new ArgumentException("账户没有服务器URL信息", nameof(account));
        }

        if (string.IsNullOrEmpty(account.McAccessToken))
        {
            throw new ArgumentException("账户没有访问令牌", nameof(account));
        }

        if (string.IsNullOrEmpty(account.ClientToken))
        {
            throw new ArgumentException("账户没有客户端令牌", nameof(account));
        }

        var baseUrl = account.ServerUrl;
        var signoutEndpoint = $"{baseUrl}/authserver/signout";

        this.LogYggdrasilInfo($"注销外置登录: {account.UserName} @ {account.ServerUrl}");

        // 构建注销请求
        var signoutRequest = new
        {
            username = account.UserName, password = string.Empty // TODO: 注销时需要提供密码，但我们没有存储密码，这里需要用户重新输入
        };

        // 由于需要密码，这里实际上不完整
        this.LogYggdrasilWarning("无法完成注销，需要用户密码");
        throw new NotImplementedException("注销功能需要用户密码，目前暂未完全支持");
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(YggdrasilAccount account, int timeoutMs = 5000)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (string.IsNullOrEmpty(account.ServerUrl))
        {
            throw new ArgumentException("账户没有服务器URL信息", nameof(account));
        }

        if (string.IsNullOrEmpty(account.McAccessToken))
        {
            throw new ArgumentException("账户没有访问令牌", nameof(account));
        }

        if (string.IsNullOrEmpty(account.ClientToken))
        {
            throw new ArgumentException("账户没有客户端令牌", nameof(account));
        }

        var baseUrl = account.ServerUrl;
        var invalidateEndpoint = $"{baseUrl}/authserver/invalidate";

        this.LogYggdrasilInfo($"使外置登录令牌失效: {account.UserName} @ {account.ServerUrl}");

        // 构建使令牌失效的请求
        var invalidateRequest = new { accessToken = account.McAccessToken, clientToken = account.ClientToken };

        try
        {
            // 配置超时时间
            Shared.HttpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            // 序列化请求体
            var requestJson = JsonSerializer.Serialize(invalidateRequest, _jsonOptions);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            // 发送请求
            var response = await Shared.HttpClient.PostAsync(invalidateEndpoint, content).ConfigureAwait(false);

            // 204 No Content表示操作成功
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                this.LogYggdrasilInfo($"已使外置登录令牌失效: {account.UserName}");
                return;
            }

            // 其他状态表示操作失败
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            this.LogYggdrasilWarning($"使外置登录令牌失效失败: {response.StatusCode} - {responseContent}");
        }
        catch (Exception ex)
        {
            this.LogYggdrasilWarning($"使外置登录令牌失效时发生异常: {ex.Message}");
            // 不抛出异常，因为这是清理操作
        }
    }

    private string GetServerName(string serverUrl)
    {
        try
        {
            var uri = new Uri(serverUrl);
            return uri.Host;
        }
        catch
        {
            return serverUrl;
        }
    }

    private string GetUserProperties(List<YggdrasilProperty>? properties)
    {
        if (properties == null) return string.Empty;

        foreach (var property in properties.Where(property =>
                     property.Name.Equals("textures", StringComparison.OrdinalIgnoreCase) &&
                     !string.IsNullOrEmpty(property.Value)))
        {
            return property.Value;
        }

        return string.Empty;
    }
}
