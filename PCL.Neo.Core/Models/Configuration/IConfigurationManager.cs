using System.Text.Json;

namespace PCL.Neo.Core.Models.Configuration;

public interface IConfigurationManager
{
    /// <summary>
    /// 获取配置项
    /// </summary>
    /// <typeparam name="TResult">目标反序列化的数据类</typeparam>
    /// <returns>返回反序列化后的配置结果，失败返回null</returns>
    TResult? GetConfiguration<TResult>() where TResult : class, new();

    /// <summary>
    /// 更新配置项的值
    /// </summary>
    /// <typeparam name="TResult">目标更新配置项的数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    /// <returns>操作是否成功</returns>
    Task<bool> UpdateConfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new();

    /// <summary>
    /// 创建配置文件
    /// </summary>
    /// <typeparam name="TResult">配置项数据类</typeparam>
    /// <param name="config">配置项对象</param>
    /// <param name="options">Json序列化选项</param>
    /// <returns>操作是否成功</returns>
    Task<bool> CreateConfiguration<TResult>(TResult config, JsonSerializerOptions? options)
        where TResult : class, new();

    /// <summary>
    /// 获取配置，如果不存在则创建默认配置
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <returns>配置对象</returns>
    Task<TResult> GetOrCreateConfiguration<TResult>() where TResult : class, new();

    /// <summary>
    /// 从指定路径加载配置
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <param name="filePath">文件路径</param>
    /// <returns>配置对象，失败返回null</returns>
    TResult? LoadFromPath<TResult>(string filePath) where TResult : class, new();

    /// <summary>
    /// 保存配置到指定路径
    /// </summary>
    /// <typeparam name="TResult">配置类型</typeparam>
    /// <param name="config">配置对象</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="options">序列化选项</param>
    /// <returns>是否成功</returns>
    Task<bool> SaveToPath<TResult>(TResult config, string filePath, JsonSerializerOptions? options = null)
        where TResult : class, new();

    /// <summary>
    /// 注册配置迁移操作
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="migrationAction">迁移操作</param>
    void RegisterMigration<T>(Action<T> migrationAction) where T : class;

    /// <summary>
    /// 备份配置文件
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <returns>是否成功</returns>
    Task<bool> BackupConfiguration<T>() where T : class, new();

    /// <summary>
    /// 获取配置访问器
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <returns>配置访问器</returns>
    ConfigurationAccessor<T> GetAccessor<T>() where T : class, new();
}
