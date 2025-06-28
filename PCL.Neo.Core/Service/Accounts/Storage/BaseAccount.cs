using System.Text.Json.Serialization;

namespace PCL.Neo.Core.Service.Accounts.Storage
{
    // Decorate with JsonPolymorphic and JsonDerivedType for System.Text.Json
    // We'll use "StorageType" as the discriminator property.
    // By default, System.Text.Json uses "$type" if not specified, but StorageType is more natural here.
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "StorageType")]
    [JsonDerivedType(typeof(MsaAccount), typeDiscriminator: UserTypeConstants.Msa)]
    [JsonDerivedType(typeof(YggdrasilAccount), typeDiscriminator: UserTypeConstants.Yggdrasil)]
    [JsonDerivedType(typeof(OfflineAccount), typeDiscriminator: UserTypeConstants.Offline)]
    public abstract record BaseAccount
    {
        // 账户基础信息
        public required string Uuid { get; init; }
        public required string UserName { get; init; }
        public string StorageType { get; }
        public string UserProperties { get; init; } = string.Empty;
        public required List<Skin> Skins { get; init; } = [];
        public required List<Cape> Capes { get; init; } = [];
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public DateTime AddedTime { get; init; } = DateTime.Now;
        
        // 关联的游戏档案ID列表
        public List<string> ProfileIds { get; set; } = [];
        
        // 当前选中的档案ID
        public string? CurrentProfileId { get; set; }
        
        // 构造函数确保设置正确的账户类型
        protected BaseAccount(string storageType)
        {
            StorageType = storageType;
        }
        
        // 检查账户是否已过期（由子类实现具体逻辑）
        public abstract bool IsExpired();
        
        // 获取账户渲染所需的显示信息
        public virtual string GetDisplayName() => UserName;
        
        public virtual string GetInitial() =>
            string.IsNullOrEmpty(UserName) ? "?" : UserName.Substring(0, 1).ToUpper();
    }
}