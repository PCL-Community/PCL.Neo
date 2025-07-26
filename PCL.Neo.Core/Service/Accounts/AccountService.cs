using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Core.Service.Accounts.Storage;
using System.Text.Json;

namespace PCL.Neo.Core.Service.Accounts;

/// <summary>
/// 统一的账户管理服务实现
/// </summary>
public class AccountService : IAccountService
{
    private readonly IMicrosoftAuthService _microsoftAuthService;
    private readonly string _accountsFilePath;
    private readonly string _selectedAccountFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    private List<BaseAccount> _cachedAccounts = [];
    private string? _selectedAccountUuid;
    private bool _isLoaded;

    public AccountService(IMicrosoftAuthService microsoftAuthService)
    {
        _microsoftAuthService = microsoftAuthService;

        // 配置路径
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCL.Neo");
        Directory.CreateDirectory(appDataPath);

        // init file path
        _accountsFilePath = Path.Combine(appDataPath, "accounts.json");
        _selectedAccountFilePath = Path.Combine(appDataPath, "selected_account.txt");

        // 配置JSON序列化选项
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        this.LogAccountInfo("账户服务已初始化");
    }

    // 确保数据已加载
    private async Task EnsureLoadedAsync()
    {
        if (_isLoaded) return;

        try
        {
            await LoadAccountsAsync().ConfigureAwait(false);
            await LoadSelectedAccountAsync().ConfigureAwait(false);

            _isLoaded = true;
            this.LogAccountDebug("账户数据已加载完成");
        }
        catch (Exception ex)
        {
            this.LogAccountError("加载账户数据失败", ex);
            throw;
        }
    }

    // 加载账户列表
    private async Task LoadAccountsAsync()
    {
        if (!File.Exists(_accountsFilePath))
        {
            this.LogAccountInfo($"账户文件不存在，将创建新的账户列表: {_accountsFilePath}");
            _cachedAccounts = [];
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_accountsFilePath);
            _cachedAccounts = JsonSerializer.Deserialize<List<BaseAccount>>(json, _jsonOptions) ?? [];
            this.LogAccountDebug($"已加载 {_cachedAccounts.Count} 个账户");
        }
        catch (JsonException ex)
        {
            // 处理文件损坏的情况
            this.LogAccountError($"账户文件解析失败，可能格式损坏: {_accountsFilePath}", ex);

            // 备份损坏的文件
            var backupPath = $"{_accountsFilePath}.{DateTime.Now:yyyyMMddHHmmss}.bak";
            try
            {
                File.Copy(_accountsFilePath, backupPath);
                this.LogAccountInfo($"已创建损坏账户文件的备份: {backupPath}");
            }
            catch (Exception backupEx)
            {
                this.LogAccountWarning($"无法创建账户文件备份: {backupEx.Message}");
            }

            _cachedAccounts = [];
        }
        catch (Exception ex)
        {
            this.LogAccountError($"加载账户文件失败: {_accountsFilePath}", ex);
            _cachedAccounts = [];
            throw;
        }
    }

    // 保存账户列表
    private async Task SaveAccountsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cachedAccounts, _jsonOptions);
            await File.WriteAllTextAsync(_accountsFilePath, json);
            this.LogAccountDebug($"已保存 {_cachedAccounts.Count} 个账户");
        }
        catch (Exception ex)
        {
            this.LogAccountError($"保存账户文件失败: {_accountsFilePath}", ex);
            throw;
        }
    }

    // 加载选中的账户
    private async Task LoadSelectedAccountAsync()
    {
        if (!File.Exists(_selectedAccountFilePath))
        {
            _selectedAccountUuid = null;
            this.LogAccountDebug("未发现选中账户的记录");
            return;
        }

        try
        {
            _selectedAccountUuid = await File.ReadAllTextAsync(_selectedAccountFilePath);

            // 如果UUID为空或不存在对应账户，则清空选择
            if (string.IsNullOrEmpty(_selectedAccountUuid) ||
                _cachedAccounts.All(a => a.Uuid != _selectedAccountUuid))
            {
                this.LogAccountWarning($"所选账户UUID不存在于账户列表中: {_selectedAccountUuid}");
                _selectedAccountUuid = null;
            }
            else
            {
                var selectedAccount = _cachedAccounts.First(a => a.Uuid == _selectedAccountUuid);
                this.LogAccountDebug($"已加载选中的账户: {selectedAccount.UserName} (类型: {selectedAccount.StorageType})");
            }
        }
        catch (Exception ex)
        {
            this.LogAccountWarning("读取选中账户记录失败", ex);
            _selectedAccountUuid = null;
        }
    }

    // 保存选中的账户
    private async Task SaveSelectedAccountAsync()
    {
        try
        {
            if (_selectedAccountUuid == null)
            {
                if (!File.Exists(_selectedAccountFilePath))
                {
                    return;
                }

                File.Delete(_selectedAccountFilePath);
                this.LogAccountDebug("已清除选中账户记录");
                return;
            }

            await File.WriteAllTextAsync(_selectedAccountFilePath, _selectedAccountUuid);
            this.LogAccountDebug($"已保存选中账户: {_selectedAccountUuid}");
        }
        catch (Exception ex)
        {
            this.LogAccountWarning("保存选中账户记录失败", ex);
        }
    }

    /// <inheritdoc />
    public async Task<List<BaseAccount>> GetAllAccountsAsync()
    {
        try
        {
            await EnsureLoadedAsync();

            // 返回按最后使用时间排序的账户列表副本
            var accounts = _cachedAccounts
                .OrderByDescending(a => a.LastUsed)
                .ToList();

            this.LogAccountDebug($"获取所有账户: {accounts.Count} 个账户");
            return accounts;
        }
        catch (Exception ex)
        {
            this.LogAccountError("获取账户列表失败", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BaseAccount?> GetAccountByUuidAsync(string uuid)
    {
        if (string.IsNullOrEmpty(uuid))
        {
            this.LogAccountWarning("尝试获取账户时提供了空UUID");
            return null;
        }

        try
        {
            await EnsureLoadedAsync();
            var account = _cachedAccounts.FirstOrDefault(a => a.Uuid == uuid);

            this.LogAccountDebug(account == null
                ? $"未找到UUID为 {uuid} 的账户"
                : $"获取账户: {account.UserName} (UUID: {uuid})");

            return account;
        }
        catch (Exception ex)
        {
            this.LogAccountError($"获取UUID为 {uuid} 的账户失败", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SaveAccountAsync(BaseAccount account)
    {
        if (account == null)
        {
            this.LogAccountError("尝试保存空账户");
            throw new ArgumentNullException(nameof(account));
        }

        try
        {
            await EnsureLoadedAsync();

            // 更新或添加账户
            var existingIndex = _cachedAccounts.FindIndex(a => a.Uuid == account.Uuid);
            if (existingIndex >= 0)
            {
                this.LogAccountDebug($"更新账户: {account.UserName} (UUID: {account.Uuid})");
                _cachedAccounts[existingIndex] = account;
            }
            else
            {
                this.LogAccountInfo($"添加新账户: {account.UserName} (类型: {account.StorageType})");
                _cachedAccounts.Add(account);
            }

            await SaveAccountsAsync();
        }
        catch (Exception ex)
        {
            this.LogAccountError($"保存账户失败: {account.UserName} (UUID: {account.Uuid})", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAccountAsync(string uuid)
    {
        if (string.IsNullOrEmpty(uuid))
        {
            this.LogAccountWarning("尝试删除账户时提供了空UUID");
            return;
        }

        try
        {
            await EnsureLoadedAsync();

            var existingIndex = _cachedAccounts.FindIndex(a => a.Uuid == uuid);
            if (existingIndex >= 0)
            {
                var accountToDelete = _cachedAccounts[existingIndex];
                this.LogAccountInfo($"删除账户: {accountToDelete.UserName} (UUID: {uuid})");

                _cachedAccounts.RemoveAt(existingIndex);

                // 如果删除的是当前选中的账户，则清除选择
                if (_selectedAccountUuid == uuid)
                {
                    this.LogAccountInfo("删除的账户是当前选中账户，已清除选择");
                    _selectedAccountUuid = null;
                    await SaveSelectedAccountAsync();
                }

                await SaveAccountsAsync();
            }
            else
            {
                this.LogAccountWarning($"尝试删除不存在的账户: UUID {uuid}");
            }
        }
        catch (Exception ex)
        {
            this.LogAccountError($"删除账户失败: UUID {uuid}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BaseAccount?> GetSelectedAccountAsync()
    {
        try
        {
            await EnsureLoadedAsync();

            if (_selectedAccountUuid == null)
            {
                this.LogAccountDebug("当前没有选中的账户");
                return null;
            }

            var account = _cachedAccounts.FirstOrDefault(a => a.Uuid == _selectedAccountUuid);
            if (account == null)
            {
                this.LogAccountWarning($"选中的账户不存在: UUID {_selectedAccountUuid}");
                return null;
            }

            this.LogAccountDebug($"获取选中账户: {account.UserName} (UUID: {_selectedAccountUuid})");
            return account;
        }
        catch (Exception ex)
        {
            this.LogAccountError("获取选中账户失败", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SetSelectedAccountAsync(string uuid)
    {
        if (string.IsNullOrEmpty(uuid))
        {
            this.LogAccountError("尝试设置选中账户时提供了空UUID");
            throw new ArgumentException("账户UUID不能为空", nameof(uuid));
        }

        try
        {
            await EnsureLoadedAsync();

            // 确认账户存在
            var account = _cachedAccounts.FirstOrDefault(a => a.Uuid == uuid);
            if (account == null)
            {
                this.LogAccountError($"尝试选择不存在的账户: UUID {uuid}");
                throw new ArgumentException($"账户不存在: {uuid}");
            }

            // 设置选中账户并更新最后使用时间
            _selectedAccountUuid = uuid;
            account.LastUsed = DateTime.Now;

            this.LogAccountInfo($"已选中账户: {account.UserName} (UUID: {uuid})");

            await SaveSelectedAccountAsync();
            await SaveAccountsAsync();
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            this.LogAccountError($"设置选中账户失败: UUID {uuid}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<OfflineAccount> CreateOfflineAccountAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            this.LogAccountError("尝试创建离线账户时提供了空用户名");
            throw new ArgumentException("用户名不能为空", nameof(username));
        }

        try
        {
            await EnsureLoadedAsync();

            if (!OfflineAccount.IsValidUsername(username))
            {
                this.LogAccountError($"无效的离线账户用户名: {username}");
                throw new ArgumentException("无效的用户名");
            }

            var account = OfflineAccount.Create(username);
            this.LogAccountInfo($"创建离线账户: {username} (UUID: {account.Uuid})");

            await SaveAccountAsync(account);
            return account;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            this.LogAccountError($"创建离线账户失败: {username}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public IObservable<DeviceFlowState> StartMicrosoftDeviceCodeFlowAsync()
    {
        this.LogAccountInfo("开始微软设备码登录流程");
        return _microsoftAuthService.StartDeviceCodeFlow();
    }

    /// <inheritdoc />
    public async Task<MsaAccount> RefreshMicrosoftAccountAsync(MsaAccount account)
    {
        if (account == null)
        {
            this.LogAccountError("尝试刷新空的微软账户");
            throw new ArgumentNullException(nameof(account));
        }

        try
        {
            if (!account.IsExpired() && !account.NeedsRefresh())
            {
                this.LogAccountDebug($"微软账户不需要刷新: {account.UserName}");
                return account;
            }

            try
            {
                this.LogAccountInfo($"开始刷新微软账户令牌: {account.UserName}");

                var refreshResult = await _microsoftAuthService.RefreshTokenAsync(account.OAuthToken.RefreshToken);

                this.LogAccountDebug("获取到新的OAuth令牌");

                // 获取Minecraft令牌
                var mcTokenResult =
                    await _microsoftAuthService.GetUserMinecraftAccessTokenAsync(refreshResult.AccessToken);

                this.LogAccountDebug("获取到新的Minecraft令牌");

                // 获取用户信息
                var accountInfoResult = await _microsoftAuthService.GetUserAccountInfoAsync(mcTokenResult);

                // 创建更新后的账户
                var updatedAccount = account with
                {
                    OAuthToken = refreshResult,
                    McAccessToken = mcTokenResult,
                    UserName = accountInfoResult.UserName,
                    Skins = accountInfoResult.Skins,
                    Capes = accountInfoResult.Capes
                };
                this.LogAccountInfo($"微软账户令牌刷新成功: {updatedAccount.UserName}");

                // 保存更新后的账户
                await SaveAccountAsync(updatedAccount);

                return updatedAccount;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            this.LogAccountError($"刷新微软账户失败: {account.UserName}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<YggdrasilAccount> ValidateYggdrasilAccountAsync(string serverUrl, string username,
        string password)
    {
        try
        {
            if (string.IsNullOrEmpty(serverUrl))
            {
                this.LogAccountError("尝试验证外置登录账户时提供了空服务器URL");
                throw new ArgumentException("服务器URL不能为空");
            }

            if (string.IsNullOrEmpty(username))
            {
                this.LogAccountError("尝试验证外置登录账户时提供了空用户名");
                throw new ArgumentException("用户名不能为空");
            }

            if (string.IsNullOrEmpty(password))
            {
                this.LogAccountError("尝试验证外置登录账户时提供了空密码");
                throw new ArgumentException("密码不能为空");
            }

            // 外置登录服务端点
            var authEndpoint = $"{serverUrl.TrimEnd('/')}/authserver/authenticate";
            this.LogAccountDebug($"外置登录认证端点: {authEndpoint}");

            // 生成客户端令牌
            var clientToken = Guid.NewGuid().ToString();

            // 构建认证请求
            var authRequest = new
            {
                username,
                password,
                clientToken,
                requestUser = true,
                agent = new { name = "Minecraft", version = 1 }
            };

            this.LogYggdrasilInfo($"开始外置登录验证: {username} @ {serverUrl}");

            // TODO: 实现与Yggdrasil服务器的实际HTTP通信
            // 以下代码仅为框架，需要替换为实际的HTTP请求逻辑

            /*
            使用HTTP客户端发送请求示例:

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(_yggdrasilApiTimeoutMs);

            if (!string.IsNullOrEmpty(_yggdrasilApiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_yggdrasilApiKey}");
            }

            var content = new StringContent(
                JsonSerializer.Serialize(authRequest, _jsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(authEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // 解析错误响应
                this.LogYggdrasilError($"外置登录验证失败: {response.StatusCode} - {errorContent}");
                throw new YggdrasilAuthException($"认证失败: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<YggdrasilAuthResponse>(responseContent, _jsonOptions);

            this.LogYggdrasilInfo($"外置登录验证成功: {authResponse.SelectedProfile.Name}");

            // 返回账户信息
            return new YggdrasilAccount
            {
                Uuid = authResponse.SelectedProfile.Id,
                UserName = authResponse.SelectedProfile.Name,
                McAccessToken = authResponse.AccessToken,
                ClientToken = authResponse.ClientToken,
                ServerUrl = serverUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 或者从响应中获取
                AddedTime = DateTime.Now,
                LastUsed = DateTime.Now
            };
            */

            // 暂时抛出未实现异常
            this.LogYggdrasilError("外置登录认证功能尚未实现");
            throw new NotImplementedException("Yggdrasil认证尚未实现");
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is NotImplementedException))
        {
            this.LogYggdrasilError($"外置登录验证过程中发生未知错误: {serverUrl}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<YggdrasilAccount> RefreshYggdrasilAccountAsync(YggdrasilAccount account)
    {
        try
        {
            if (account == null)
            {
                this.LogAccountError("尝试刷新空的外置登录账户");
                throw new ArgumentNullException(nameof(account));
            }

            if (string.IsNullOrEmpty(account.ServerUrl))
            {
                this.LogAccountError("尝试刷新外置登录账户时服务器URL为空");
                throw new ArgumentException("服务器URL不能为空");
            }

            // 外置登录刷新端点
            var refreshEndpoint = $"{account.ServerUrl.TrimEnd('/')}/authserver/refresh";
            this.LogYggdrasilDebug($"外置登录刷新端点: {refreshEndpoint}");

            // 构建刷新请求
            var refreshRequest = new
            {
                accessToken = account.McAccessToken,
                clientToken = account.ClientToken
            };

            this.LogYggdrasilInfo($"开始刷新外置登录令牌: {account.UserName} @ {account.ServerUrl}");

            // TODO: 实现与Yggdrasil服务器的实际HTTP通信
            // 以下代码仅为框架，需要替换为实际的HTTP请求逻辑

            /*
            使用HTTP客户端发送请求示例:

            var client = new HttpClient();
            client.Timeout = TimeSpan.FromMilliseconds(_yggdrasilApiTimeoutMs);

            if (!string.IsNullOrEmpty(_yggdrasilApiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_yggdrasilApiKey}");
            }

            var content = new StringContent(
                JsonSerializer.Serialize(refreshRequest, _jsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(refreshEndpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // 解析错误响应
                this.LogYggdrasilError($"刷新外置登录令牌失败: {response.StatusCode} - {errorContent}");
                throw new YggdrasilAuthException($"刷新令牌失败: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var refreshResponse = JsonSerializer.Deserialize<YggdrasilRefreshResponse>(responseContent, _jsonOptions);

            this.LogYggdrasilInfo($"外置登录令牌刷新成功: {account.UserName}");

            // 返回更新后的账户
            return account with
            {
                McAccessToken = refreshResponse.AccessToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24), // 或者从响应中获取
                LastUsed = DateTime.Now
            };
            */

            // 暂时抛出未实现异常
            this.LogYggdrasilError("外置登录令牌刷新功能尚未实现");
            throw new NotImplementedException("Yggdrasil令牌刷新尚未实现");
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is ArgumentNullException ||
                                     ex is NotImplementedException))
        {
            this.LogYggdrasilError($"刷新外置登录令牌过程中发生未知错误: {account?.UserName ?? "未知账户"}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> GetProfileIdsForAccountAsync(string uuid)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountWarning("尝试获取档案IDs时提供了空的账户UUID");
                return [];
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountWarning($"尝试获取不存在账户的档案IDs: UUID {uuid}");
                return [];
            }

            this.LogProfileDebug($"获取账户关联的档案IDs: {account.UserName}, 共 {account.ProfileIds.Count} 个档案");
            return account.ProfileIds;
        }
        catch (Exception ex)
        {
            this.LogProfileError($"获取账户档案IDs失败: UUID {uuid}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task AddProfileToAccountAsync(string uuid, string profileId)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountError("尝试添加档案时提供了空的账户UUID");
                throw new ArgumentException("账户UUID不能为空", nameof(uuid));
            }

            if (string.IsNullOrEmpty(profileId))
            {
                this.LogAccountError("尝试添加档案时提供了空的档案ID");
                throw new ArgumentException("档案ID不能为空", nameof(profileId));
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountError($"尝试向不存在的账户添加档案: UUID {uuid}");
                throw new ArgumentException($"账户不存在: {uuid}");
            }

            if (!account.ProfileIds.Contains(profileId))
            {
                var updatedAccount = account with {};
                updatedAccount.ProfileIds.Add(profileId);

                this.LogProfileInfo($"向账户 {account.UserName} 添加档案 {profileId}");

                if (updatedAccount.CurrentProfileId == null)
                {
                    updatedAccount.CurrentProfileId = profileId;
                    this.LogProfileInfo($"自动设置账户 {account.UserName} 的当前档案为 {profileId}");
                }

                await SaveAccountAsync(updatedAccount);
            }
            else
            {
                this.LogProfileWarning($"档案 {profileId} 已关联到账户 {account.UserName}");
            }
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            this.LogProfileError($"向账户添加档案失败: UUID {uuid}, ProfileID {profileId}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RemoveProfileFromAccountAsync(string uuid, string profileId)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountWarning("尝试移除档案时提供了空的账户UUID");
                return;
            }

            if (string.IsNullOrEmpty(profileId))
            {
                this.LogAccountWarning("尝试移除档案时提供了空的档案ID");
                return;
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountWarning($"尝试从不存在的账户移除档案: UUID {uuid}");
                return;
            }

            if (account.ProfileIds.Contains(profileId))
            {
                var updatedAccount = account with {};
                updatedAccount.ProfileIds.Remove(profileId);

                this.LogProfileInfo($"从账户 {account.UserName} 移除档案 {profileId}");

                if (account.CurrentProfileId == profileId)
                {
                    updatedAccount.CurrentProfileId = updatedAccount.ProfileIds.FirstOrDefault();
                    this.LogProfileInfo($"重置账户 {account.UserName} 的当前档案为 {updatedAccount.CurrentProfileId ?? "无"}");
                }

                await SaveAccountAsync(updatedAccount);
            }
            else
            {
                this.LogProfileWarning($"账户 {account.UserName} 未关联档案 {profileId}");
            }
        }
        catch (Exception ex)
        {
            this.LogProfileError($"从账户移除档案失败: UUID {uuid}, ProfileID {profileId}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetCurrentProfileIdForAccountAsync(string uuid)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountWarning("尝试获取当前档案ID时提供了空的账户UUID");
                return null;
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountWarning($"尝试获取不存在账户的当前档案ID: UUID {uuid}");
                return null;
            }

            this.LogProfileDebug($"获取账户 {account.UserName} 的当前档案ID: {account.CurrentProfileId ?? "无"}");
            return account.CurrentProfileId;
        }
        catch (Exception ex)
        {
            this.LogProfileError($"获取账户当前档案ID失败: UUID {uuid}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task SetCurrentProfileIdForAccountAsync(string uuid, string profileId)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountError("尝试设置当前档案时提供了空的账户UUID");
                throw new ArgumentException("账户UUID不能为空", nameof(uuid));
            }

            if (string.IsNullOrEmpty(profileId))
            {
                this.LogAccountError("尝试设置当前档案时提供了空的档案ID");
                throw new ArgumentException("档案ID不能为空", nameof(profileId));
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountError($"尝试为不存在的账户设置当前档案: UUID {uuid}");
                throw new ArgumentException($"账户不存在: {uuid}");
            }

            if (!account.ProfileIds.Contains(profileId))
            {
                this.LogProfileInfo($"自动添加档案 {profileId} 到账户 {account.UserName}");
                account.ProfileIds.Add(profileId);
            }

            var updatedAccount = account with
            {
                CurrentProfileId = profileId
            };

            this.LogProfileInfo($"设置账户 {account.UserName} 的当前档案为 {profileId}");

            await SaveAccountAsync(updatedAccount);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            this.LogProfileError($"设置账户当前档案失败: UUID {uuid}, ProfileID {profileId}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> IsAccountRefreshRequiredAsync(string uuid)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountWarning("尝试检查账户是否需要刷新时提供了空UUID");
                return false;
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountWarning($"尝试检查不存在账户是否需要刷新: UUID {uuid}");
                throw new ArgumentException($"账户不存在: {uuid}");
            }

            var needsRefresh = account switch
            {
                MsaAccount msaAccount => msaAccount.IsExpired() || msaAccount.NeedsRefresh(),
                YggdrasilAccount yggdrasilAccount => yggdrasilAccount.IsExpired() || yggdrasilAccount.NeedsRefresh(),
                _ => false // 离线账户不需要刷新
            };

            this.LogAccountDebug($"检查账户 {account.UserName} 是否需要刷新: {needsRefresh}");
            return needsRefresh;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            this.LogAccountError($"检查账户是否需要刷新失败: UUID {uuid}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<BaseAccount> RefreshAccountIfNeededAsync(string uuid)
    {
        try
        {
            if (string.IsNullOrEmpty(uuid))
            {
                this.LogAccountError("尝试刷新账户时提供了空UUID");
                throw new ArgumentException("账户UUID不能为空", nameof(uuid));
            }

            var account = await GetAccountByUuidAsync(uuid);
            if (account == null)
            {
                this.LogAccountError($"尝试刷新不存在的账户: UUID {uuid}");
                throw new ArgumentException($"账户不存在: {uuid}");
            }

            if (!await IsAccountRefreshRequiredAsync(uuid))
            {
                this.LogAccountDebug($"账户 {account.UserName} 不需要刷新");
                return account;
            }

            this.LogAccountInfo($"开始刷新账户 {account.UserName} (类型: {account.StorageType})");

            var refreshedAccount = account switch
            {
                MsaAccount msaAccount => await RefreshMicrosoftAccountAsync(msaAccount),
                YggdrasilAccount yggdrasilAccount => await RefreshYggdrasilAccountAsync(yggdrasilAccount),
                _ => account // 离线账户不需要刷新
            };

            this.LogAccountInfo($"账户 {refreshedAccount.UserName} 刷新完成");
            return refreshedAccount;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            this.LogAccountError($"刷新账户失败: UUID {uuid}", ex);
            throw;
        }
    }
}
