using PCL.Neo.Core.Service.Accounts.Storage;
using System;
using System.Linq;

namespace PCL.Neo.Models.User;

public class UserInfo
{
    private readonly BaseAccount _account;

    public string Uuid => _account.Uuid;
    public string Username => _account.UserName;
    public DateTime LastUsed => _account.LastUsed;
    public DateTime AddedTime => _account.AddedTime;

    public string StorageType => _account.StorageType;

    public BaseAccount Account => _account;

    public UserInfo(BaseAccount account)
    {
        _account = account ?? throw new ArgumentNullException(nameof(account));
    }

    public string GetUserTypeText()
    {
        return _account.StorageType switch
        {
            UserTypeConstants.Msa => "微软账户",
            UserTypeConstants.Yggdrasil => "外置登录",
            UserTypeConstants.Offline => "离线账户",
            _ => "未知类型"
        };
    }

    public string GetInitial()
    {
        return _account.GetInitial();
    }

    public bool HasSkin()
    {
        return _account.Skins.Any();
    }

    public bool HasCape()
    {
        return _account.Capes.Any();
    }

    public string GetSkinUrl()
    {
        if (_account.Skins.Count > 0)
        {
            return _account.Skins[0].Url.ToString();
        }

        return string.Empty;
    }
}
