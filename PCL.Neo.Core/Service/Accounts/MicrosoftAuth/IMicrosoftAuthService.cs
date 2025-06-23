using PCL.Neo.Core.Service.Accounts.Exceptions;
using PCL.Neo.Core.Service.Accounts.OAuthService.Exceptions;
using PCL.Neo.Core.Utils;

namespace PCL.Neo.Core.Service.Accounts.MicrosoftAuth;

public interface IMicrosoftAuthService
{
    /// <summary>
    /// 开始设备码登录流程
    /// </summary>
    /// <returns>设备码登录流状态</returns>
    IObservable<DeviceFlowState> StartDeviceCodeFlow();

    /// <summary>
    /// 获取设备码
    /// </summary>
    /// <returns>获取到的设备码信息，会在失败的时候返回异常</returns>
    Task<Result<DeviceCodeData.DeviceCodeInfo, HttpError>> RequestDeviceCodeAsync();

    /// <summary>
    /// 开始轮询服务器
    /// </summary>
    /// <param name="deviceCode">设备码</param>
    /// <param name="interval">轮询间隔</param>
    /// <returns>轮询结果</returns>
    Task<Result<DeviceCodeData.DeviceCodeAccessToken, DeviceFlowError>> PollForTokenAsync(string deviceCode,
        int interval);

    /// <summary>
    /// 获取玩家全部信息
    /// </summary>
    /// <param name="accessToken">通行Token</param>
    /// <returns>玩家信息</returns>
    Task<Result<string, NotHaveGameException>> GetUserMinecraftAccessTokenAsync(string accessToken);

    /// <summary>
    /// 获取玩家的账户信息
    /// </summary>
    /// <param name="accessToken">需要的Token</param>
    /// <returns>账户信息</returns>
    Task<Result<DeviceCodeData.McAccountInfo, Exception>> GetUserAccountInfoAsync(string accessToken);

    /// <summary>
    /// 刷新玩家的OAuth2 Token
    /// </summary>
    /// <param name="refreshToken">OAuth2的刷新Token</param>
    /// <returns>新的Token</returns>
    Task<Result<Storage.OAuthTokenData, Exception>> RefreshTokenAsync(string refreshToken);
}