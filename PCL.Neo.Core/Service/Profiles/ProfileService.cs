using PCL.Neo.Core.Models.Profile;
using PCL.Neo.Core.Service.Accounts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PCL.Neo.Core.Service.Profiles
{
    /// <summary>
    /// 游戏档案管理服务实现
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IAccountService       _accountService;
        private readonly string                _profilesFilePath;
        private readonly string                _profileTemplatesFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        private List<GameProfile> _cachedProfiles  = [];
        private List<GameProfile> _cachedTemplates = [];
        private bool              _isLoaded        = false;

        public ProfileService(IAccountService accountService)
        {
            _accountService = accountService;
            
            // 配置路径
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCL.Neo");
            Directory.CreateDirectory(appDataPath);
            _profilesFilePath = Path.Combine(appDataPath, "profiles.json");
            _profileTemplatesFilePath = Path.Combine(appDataPath, "profile_templates.json");
            
            // 配置JSON序列化选项
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }
        
        // 确保数据已加载
        private async Task EnsureLoadedAsync()
        {
            if (_isLoaded) return;

            await LoadProfilesAsync().ConfigureAwait(false);
            await LoadTemplatesAsync().ConfigureAwait(false);

            _isLoaded = true;
        }
        
        // 加载档案列表
        private async Task LoadProfilesAsync()
        {
            if (!File.Exists(_profilesFilePath))
            {
                _cachedProfiles = CreateDefaultProfiles();
                await SaveProfilesAsync().ConfigureAwait(false);
                return;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(_profilesFilePath);
                _cachedProfiles = JsonSerializer.Deserialize<List<GameProfile>>(json, _jsonOptions) ?? new List<GameProfile>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载档案文件失败: {ex.Message}");
                _cachedProfiles = CreateDefaultProfiles();
                await SaveProfilesAsync().ConfigureAwait(false);
            }
        }
        
        // 加载模板列表
        private async Task LoadTemplatesAsync()
        {
            if (!File.Exists(_profileTemplatesFilePath))
            {
                _cachedTemplates = CreateDefaultTemplates();
                await SaveTemplatesAsync();
                return;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(_profileTemplatesFilePath);
                _cachedTemplates = JsonSerializer.Deserialize<List<GameProfile>>(json, _jsonOptions) ?? CreateDefaultTemplates();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载档案模板文件失败: {ex.Message}");
                _cachedTemplates = CreateDefaultTemplates();
                await SaveTemplatesAsync();
            }
        }
        
        // 保存档案列表
        private async Task SaveProfilesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_cachedProfiles, _jsonOptions);
                await File.WriteAllTextAsync(_profilesFilePath, json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存档案文件失败: {ex.Message}");
                throw;
            }
        }
        
        // 保存模板列表
        private async Task SaveTemplatesAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(_cachedTemplates, _jsonOptions);
                await File.WriteAllTextAsync(_profileTemplatesFilePath, json).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存档案模板文件失败: {ex.Message}");
                throw;
            }
        }
        
        // 创建默认档案
        private List<GameProfile> CreateDefaultProfiles()
        {
            return
            [
                new GameProfile
                {
                    Id          = Guid.NewGuid().ToString(),
                    Name        = "默认档案",
                    Description = "PCL.Neo默认游戏档案",
                    GameVersion = "release"
                }
            ];
        }
        
        // 创建默认模板
        private List<GameProfile> CreateDefaultTemplates()
        {
            return
            [
                new GameProfile
                {
                    Id          = Guid.NewGuid().ToString(),
                    Name        = "原版最新版",
                    Description = "Minecraft最新稳定版",
                    GameVersion = "release",
                    IsPreset    = true
                },

                new GameProfile
                {
                    Id           = Guid.NewGuid().ToString(),
                    Name         = "Forge模组开发",
                    Description  = "适合Forge模组开发的环境配置",
                    GameVersion  = "1.20.1",
                    GameType     = "Forge",
                    JavaMemoryMb = 4096,
                    IsPreset     = true
                },

                new GameProfile
                {
                    Id             = Guid.NewGuid().ToString(),
                    Name           = "高性能游戏",
                    Description    = "针对高性能电脑优化的配置",
                    GameVersion    = "release",
                    JavaMemoryMb   = 8192,
                    GameResolution = "1920x1080",
                    FullScreen     = true,
                    IsPreset       = true
                }
            ];
        }
        
        /// <inheritdoc />
        public async Task<List<GameProfile>> GetAllProfilesAsync()
        {
            await EnsureLoadedAsync();
            return _cachedProfiles.Where(p => !p.IsPreset).ToList();
        }
        
        /// <inheritdoc />
        public async Task<GameProfile?> GetProfileByIdAsync(string id)
        {
            await EnsureLoadedAsync();
            return _cachedProfiles.FirstOrDefault(p => p.Id == id);
        }
        
        /// <inheritdoc />
        public async Task SaveProfileAsync(GameProfile profile)
        {
            await EnsureLoadedAsync();
            
            // 更新或添加档案
            var existingIndex = _cachedProfiles.FindIndex(p => p.Id == profile.Id);
            if (existingIndex >= 0)
            {
                _cachedProfiles[existingIndex] = profile;
            }
            else
            {
                _cachedProfiles.Add(profile);
            }
            
            await SaveProfilesAsync();
        }
        
        /// <inheritdoc />
        public async Task DeleteProfileAsync(string id)
        {
            await EnsureLoadedAsync();
            
            // 找到需要删除的档案
            var existingIndex = _cachedProfiles.FindIndex(p => p.Id == id);
            if (existingIndex < 0) return;
            
            // 从所有账户中解除关联
            var allAccounts = await _accountService.GetAllAccountsAsync();
            foreach (var account in allAccounts)
            {
                if (account.ProfileIds.Contains(id))
                {
                    var updatedAccount = account with { };
                    updatedAccount.ProfileIds.Remove(id);
                    
                    // 如果删除的是当前选中的档案，清除选择
                    if (account.CurrentProfileId == id)
                    {
                        updatedAccount.CurrentProfileId = updatedAccount.ProfileIds.FirstOrDefault();
                    }
                    
                    await _accountService.SaveAccountAsync(updatedAccount);
                }
            }
            
            // 删除档案
            _cachedProfiles.RemoveAt(existingIndex);
            await SaveProfilesAsync();
        }
        
        /// <inheritdoc />
        public async Task<List<GameProfile>> GetProfilesByAccountAsync(string accountUuid)
        {
            await EnsureLoadedAsync();
            
            var account = await _accountService.GetAccountByUuidAsync(accountUuid);
            if (account == null) return new List<GameProfile>();
            
            return _cachedProfiles
                .Where(p => account.ProfileIds.Contains(p.Id))
                .OrderByDescending(p => p.LastUsed)
                .ToList();
        }
        
        /// <inheritdoc />
        public async Task<GameProfile?> GetCurrentProfileForAccountAsync(string accountUuid)
        {
            await EnsureLoadedAsync();
            
            var account = await _accountService.GetAccountByUuidAsync(accountUuid);
            if (account == null || account.CurrentProfileId == null) return null;
            
            return _cachedProfiles.FirstOrDefault(p => p.Id == account.CurrentProfileId);
        }
        
        /// <inheritdoc />
        public async Task SetCurrentProfileForAccountAsync(string accountUuid, string profileId)
        {
            await EnsureLoadedAsync();
            
            // 确认账户和档案存在
            var account = await _accountService.GetAccountByUuidAsync(accountUuid);
            var profile = await GetProfileByIdAsync(profileId);
            
            if (account == null)
                throw new ArgumentException($"账户不存在: {accountUuid}");
                
            if (profile == null)
                throw new ArgumentException($"档案不存在: {profileId}");
                
            // 确保账户已关联此档案
            if (!account.ProfileIds.Contains(profileId))
            {
                account.ProfileIds.Add(profileId);
            }
            
            // 设置当前档案
            var updatedAccount = account with
            {
                CurrentProfileId = profileId
            };
            
            // 更新档案使用时间
            profile.LastUsed = DateTime.Now;
            await SaveProfileAsync(profile);
            
            // 保存账户
            await _accountService.SaveAccountAsync(updatedAccount);
        }
        
        /// <inheritdoc />
        public async Task AssociateProfileWithAccountAsync(string accountUuid, string profileId)
        {
            await EnsureLoadedAsync();
            
            // 确认账户和档案存在
            var account = await _accountService.GetAccountByUuidAsync(accountUuid);
            var profile = await GetProfileByIdAsync(profileId);
            
            if (account == null)
                throw new ArgumentException($"账户不存在: {accountUuid}");
                
            if (profile == null)
                throw new ArgumentException($"档案不存在: {profileId}");
                
            // 如果尚未关联，添加关联
            if (!account.ProfileIds.Contains(profileId))
            {
                var updatedAccount = account with { };
                updatedAccount.ProfileIds.Add(profileId);
                
                // 如果是首个档案，设为当前选中
                if (account.CurrentProfileId == null)
                {
                    updatedAccount.CurrentProfileId = profileId;
                }
                
                await _accountService.SaveAccountAsync(updatedAccount);
            }
        }
        
        /// <inheritdoc />
        public async Task DisassociateProfileFromAccountAsync(string accountUuid, string profileId)
        {
            await EnsureLoadedAsync();
            
            var account = await _accountService.GetAccountByUuidAsync(accountUuid);
            if (account == null) return;
            
            // 如果存在关联，移除关联
            if (account.ProfileIds.Contains(profileId))
            {
                var updatedAccount = account with { };
                updatedAccount.ProfileIds.Remove(profileId);
                
                // 如果移除的是当前选中的档案，选择另一个档案
                if (account.CurrentProfileId == profileId)
                {
                    updatedAccount.CurrentProfileId = updatedAccount.ProfileIds.FirstOrDefault();
                }
                
                await _accountService.SaveAccountAsync(updatedAccount);
            }
        }
        
        /// <inheritdoc />
        public async Task<GameProfile> CreateProfileAsync(string name, string description = "")
        {
            await EnsureLoadedAsync();
            
            var profile = new GameProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = description
            };
            
            await SaveProfileAsync(profile);
            return profile;
        }
        
        /// <inheritdoc />
        public async Task<GameProfile> DuplicateProfileAsync(string sourceId, string newName)
        {
            await EnsureLoadedAsync();
            
            var sourceProfile = await GetProfileByIdAsync(sourceId);
            if (sourceProfile == null)
                throw new ArgumentException($"源档案不存在: {sourceId}");
                
            var newProfile = sourceProfile.CloneProfile(newName);
            await SaveProfileAsync(newProfile);
            
            return newProfile;
        }
        
        /// <inheritdoc />
        public async Task<List<GameProfile>> GetProfileTemplatesAsync()
        {
            await EnsureLoadedAsync();
            return _cachedTemplates.ToList();
        }
    }
} 