using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Core.Service.Accounts.Storage;

namespace PCL.Neo.Core.Service.Accounts;

/// <summary>
/// 统一的账户管理服务接口
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// 获取所有账户
    /// </summary>
    Task<List<BaseAccount>> GetAllAccountsAsync();

    /// <summary>
    /// 根据UUID获取账户
    /// </summary>
    Task<BaseAccount?> GetAccountByUuidAsync(string uuid);

    /// <summary>
    /// 保存账户
    /// </summary>
    Task SaveAccountAsync(BaseAccount account);

    /// <summary>
    /// 删除账户
    /// </summary>
    Task DeleteAccountAsync(string uuid);

    /// <summary>
    /// 获取当前选中的账户
    /// </summary>
    Task<BaseAccount?> GetSelectedAccountAsync();

    /// <summary>
    /// 设置当前选中的账户
    /// </summary>
    Task SetSelectedAccountAsync(string uuid);

    /// <summary>
    /// 创建离线账户
    /// </summary>
    Task<OfflineAccount> CreateOfflineAccountAsync(string username);

    /// <summary>
    /// 开始微软设备码流程登录
    /// </summary>
    IObservable<DeviceFlowState> StartMicrosoftDeviceCodeFlowAsync();

    /// <summary>
    /// 刷新微软账户令牌
    /// </summary>
    Task<MsaAccount> RefreshMicrosoftAccountAsync(MsaAccount account);

    /// <summary>
    /// 验证Yggdrasil账户
    /// </summary>
    Task<YggdrasilAccount> ValidateYggdrasilAccountAsync(string serverUrl, string username, string password);

    /// <summary>
    /// 刷新Yggdrasil账户令牌
    /// </summary>
    Task<YggdrasilAccount> RefreshYggdrasilAccountAsync(YggdrasilAccount account);

    /// <summary>
    /// 获取账户关联的档案ID列表
    /// </summary>
    Task<List<string>> GetProfileIdsForAccountAsync(string uuid);

    /// <summary>
    /// 添加档案到账户
    /// </summary>
    Task AddProfileToAccountAsync(string uuid, string profileId);

    /// <summary>
    /// 从账户移除档案
    /// </summary>
    Task RemoveProfileFromAccountAsync(string uuid, string profileId);

    /// <summary>
    /// 获取账户当前选中的档案ID
    /// </summary>
    Task<string?> GetCurrentProfileIdForAccountAsync(string uuid);

    /// <summary>
    /// 设置账户当前选中的档案ID
    /// </summary>
    Task SetCurrentProfileIdForAccountAsync(string uuid, string profileId);

    /// <summary>
    /// 判断账户是否已过期并需要刷新
    /// </summary>
    Task<bool> IsAccountRefreshRequiredAsync(string uuid);

    /// <summary>
    /// 自动刷新账户令牌（根据类型）
    /// </summary>
    Task<BaseAccount> RefreshAccountIfNeededAsync(string uuid);
}
