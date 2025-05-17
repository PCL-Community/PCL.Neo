using System;
using System.Collections.Generic;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    public record YggdrasilAccount : BaseAccount
    {
        // Yggdrasil账户特有属性
        public required string McAccessToken { get; init; }
        public required string ClientToken { get; init; }
        public required string ServerUrl { get; init; }
        public string ServerName { get; init; } = string.Empty;
        public DateTime? ExpiresAt { get; init; }
        
        public YggdrasilAccount() : base(UserTypeConstants.Yggdrasil)
        {
        }
        
        // 检查账户是否过期
        public override bool IsExpired()
        {
            // 如果有设置过期时间且已过期，则账户过期
            return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
        }
        
        // 判断是否需要刷新令牌（令牌即将过期时提前刷新）
        public bool NeedsRefresh()
        {
            // 如果令牌将在30分钟内过期，则需要刷新
            return ExpiresAt.HasValue && DateTime.UtcNow.AddMinutes(30) > ExpiresAt.Value;
        }
    }
}