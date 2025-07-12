namespace PCL.Neo.Core.Models.Configuration;

/// <summary>
/// 配置访问器，提供统一的配置访问接口
/// </summary>
/// <typeparam name="T">配置类型</typeparam>
public class ConfigurationAccessor<T> where T : class, new()
{
    private readonly ConfigurationManager _manager;
    private T? _currentConfig;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="manager">配置管理器</param>
    internal ConfigurationAccessor(ConfigurationManager manager)
    {
        _manager = manager;
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns>配置对象</returns>
    public async Task<T> GetConfigAsync()
    {
        if (_currentConfig == null)
        {
            _currentConfig = await _manager.GetOrCreateConfiguration<T>();
        }

        return _currentConfig;
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> SaveAsync()
    {
        if (_currentConfig == null)
        {
            return false;
        }

        return await _manager.UpdateConfiguration(_currentConfig, null);
    }


    /// <summary>
    /// 更新配置
    /// </summary>
    /// <param name="updateAction">更新操作</param>
    /// <returns>是否成功</returns>
    public async Task<bool> UpdateAsync(Action<T> updateAction)
    {
        var config = await GetConfigAsync();
        updateAction(config);
        return await SaveAsync();
    }

    /// <summary>
    /// 备份配置
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> BackupAsync()
    {
        return await _manager.BackupConfiguration<T>();
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    /// <returns>是否成功</returns>
    public async Task<bool> ResetToDefaultAsync()
    {
        _currentConfig = new T();
        return await SaveAsync();
    }
}
