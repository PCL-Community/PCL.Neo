using System.Text.Json;
using PCL.Neo.Core.Models.Configuration;
using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PCL.Neo.Tests.Core.Models.Configuration
{
    [TestFixture]
    [TestOf(typeof(ConfigurationManager))]
    public class ConfigurationManagerTest
    {
        [ConfigurationInfo("testConfig.json")]
        public class TestConfiguration
        {
            public string Name { get; set; } = "TestConfig";
            public int Value { get; set; } = 42;
        }


        [Test]
        public async Task ConfigurationTest()
        {
            ConfigurationManager manager = new();
            var config = new TestConfiguration();

            // 创建配置
            bool createResult = await manager.CreateConfiguration(config, null);
            Assert.That(createResult, Is.True, "Failed to create configuration");
            
            // 获取配置
            var loadedConfig = manager.GetConfiguration<TestConfiguration>();
            Assert.That(loadedConfig, Is.Not.Null, "Failed to load configuration");
            
            var content = JsonSerializer.Serialize(loadedConfig, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine(content);

            // 更新配置
            loadedConfig.Name = "Hello World!";
            bool updateResult = await manager.UpdateConfiguration(loadedConfig, null);
            Assert.That(updateResult, Is.True, "Failed to update configuration");

            // 重新获取并验证
            var updatedConfig = manager.GetConfiguration<TestConfiguration>();
            Assert.That(updatedConfig?.Name, Is.EqualTo("Hello World!"), "Configuration update failed");
            
            content = JsonSerializer.Serialize(updatedConfig, new JsonSerializerOptions() { WriteIndented = true });
            Console.WriteLine(content);
            
            // 测试GetOrCreateConfiguration
            var autoConfig = await manager.GetOrCreateConfiguration<TestConfiguration>();
            Assert.That(autoConfig, Is.Not.Null, "GetOrCreateConfiguration failed");
        }
    }
}