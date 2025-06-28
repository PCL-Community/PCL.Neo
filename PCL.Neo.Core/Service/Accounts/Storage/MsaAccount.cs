namespace PCL.Neo.Core.Service.Accounts.Storage
{
    public record MsaAccount : BaseAccount
    {
        // 微软账户特有属性
        public required OAuthTokenData OAuthToken { get; init; }
        public required string McAccessToken { get; init; }
        
        public MsaAccount() : base(UserTypeConstants.Msa)
        {
        }
        
        // 检查账户是否过期
        public override bool IsExpired()
        {
            // 如果OAuth令牌已过期，则账户过期
            return DateTime.UtcNow > OAuthToken.ExpiresAt;
        }
        
        // 判断是否需要刷新令牌（令牌即将过期时提前刷新）
        public bool NeedsRefresh()
        {
            // 如果令牌将在30分钟内过期，则需要刷新
            return DateTime.UtcNow.AddMinutes(30) > OAuthToken.ExpiresAt;
        }
    }
}