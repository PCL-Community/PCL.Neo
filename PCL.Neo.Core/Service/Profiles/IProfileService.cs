using PCL.Neo.Core.Models.Profile;

namespace PCL.Neo.Core.Service.Profiles;

/// <summary>
/// 游戏档案管理服务接口
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// 获取所有游戏档案
    /// </summary>
    Task<List<GameProfile>> GetAllProfilesAsync();

    /// <summary>
    /// 根据ID获取游戏档案
    /// </summary>
    Task<GameProfile?> GetProfileByIdAsync(string id);

    /// <summary>
    /// 保存或更新游戏档案
    /// </summary>
    Task SaveProfileAsync(GameProfile profile);

    /// <summary>
    /// 删除游戏档案
    /// </summary>
    Task DeleteProfileAsync(string id);

    /// <summary>
    /// 获取账户关联的所有档案
    /// </summary>
    Task<List<GameProfile>> GetProfilesByAccountAsync(string accountUuid);

    /// <summary>
    /// 获取账户当前选中的档案
    /// </summary>
    Task<GameProfile?> GetCurrentProfileForAccountAsync(string accountUuid);

    /// <summary>
    /// 设置账户当前选中的档案
    /// </summary>
    Task SetCurrentProfileForAccountAsync(string accountUuid, string profileId);

    /// <summary>
    /// 关联账户和档案
    /// </summary>
    Task AssociateProfileWithAccountAsync(string accountUuid, string profileId);

    /// <summary>
    /// 解除账户和档案的关联
    /// </summary>
    Task DisassociateProfileFromAccountAsync(string accountUuid, string profileId);

    /// <summary>
    /// 创建新的游戏档案
    /// </summary>
    Task<GameProfile> CreateProfileAsync(string name, string description = "");

    /// <summary>
    /// 复制现有档案
    /// </summary>
    Task<GameProfile> DuplicateProfileAsync(string sourceId, string newName);

    /// <summary>
    /// 获取预设档案模板列表
    /// </summary>
    Task<List<GameProfile>> GetProfileTemplatesAsync();
}
