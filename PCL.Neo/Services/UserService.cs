using PCL.Neo.Core.Service.Accounts;
using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Core.Service.Accounts.Storage;
using PCL.Neo.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

/// <summary>
/// 用户服务，提供与账户系统交互的功能
/// </summary>
public class UserService
{
    private readonly IAccountService _accountService;
    private readonly List<UserInfo> _users = [];

    /// <summary>
    /// 当前用户改变事件
    /// </summary>
    public event Action<UserInfo?>? CurrentUserChanged;

    /// <summary>
    /// 当前用户
    /// </summary>
    public UserInfo? CurrentUser { get; private set; }

    /// <summary>
    /// 用户列表
    /// </summary>
    public IReadOnlyList<UserInfo> Users => _users.AsReadOnly();

    public UserService(IAccountService accountService)
    {
        _accountService = accountService;

        // 初始化用户列表
        InitializeUsers();
    }

    private void InitializeUsers()
    {
        // 使用异步加载但不等待，让UI可以立即显示
        LoadUsersAsync().ConfigureAwait(false);
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            _users.Clear();

            var accounts = await _accountService.GetAllAccountsAsync();
            foreach (var account in accounts)
            {
                _users.Add(new UserInfo(account));
            }

            // 设置当前用户
            var currentAccount = await _accountService.GetSelectedAccountAsync();
            if (currentAccount != null)
            {
                CurrentUser = _users.FirstOrDefault(u => u.Uuid == currentAccount.Uuid);
                CurrentUserChanged?.Invoke(CurrentUser);
            }
        }
        catch (Exception ex)
        {
            // 在实际应用中，应该记录日志
            Console.WriteLine($"加载用户列表失败: {ex.Message}");
        }
    }

    private void OnCurrentAccountChanged(BaseAccount? account)
    {
        CurrentUser = account != null ? _users.FirstOrDefault(u => u.Uuid == account.Uuid) : null;

        CurrentUserChanged?.Invoke(CurrentUser);
    }

    /// <summary>
    /// 切换当前用户
    /// </summary>
    public async Task SwitchUser(UserInfo user)
    {
        await _accountService.SetSelectedAccountAsync(user.Account.Uuid).ConfigureAwait(false);
        // 更新当前用户
        CurrentUser = user;
        CurrentUserChanged?.Invoke(CurrentUser);
    }

    /// <summary>
    /// 添加离线用户
    /// </summary>
    public async Task<UserInfo> AddUserAsync(string username)
    {
        var account = await _accountService.CreateOfflineAccountAsync(username);

        // 刷新用户列表
        await LoadUsersAsync();

        // 返回新添加的用户
        return _users.First(u => u.Uuid == account.Uuid);
    }

    /// <summary>
    /// 添加外置登录用户
    /// </summary>
    public async Task<UserInfo> AddYggdrasilUserAsync(string serverUrl, string username, string password)
    {
        var account = await _accountService.ValidateYggdrasilAccountAsync(serverUrl, username, password);
        await _accountService.SaveAccountAsync(account);

        // 刷新用户列表
        await LoadUsersAsync();

        // 返回新添加的用户
        return _users.First(u => u.Uuid == account.Uuid);
    }

    /// <summary>
    /// 开始微软账户登录流程
    /// </summary>
    public IObservable<DeviceFlowState> StartMicrosoftLoginAsync()
    {
        return _accountService.StartMicrosoftDeviceCodeFlowAsync();
    }

    /// <summary>
    /// 移除用户
    /// </summary>
    public async Task RemoveUserAsync(string uuid)
    {
        await _accountService.DeleteAccountAsync(uuid);

        // 刷新用户列表
        await LoadUsersAsync();
    }
}
