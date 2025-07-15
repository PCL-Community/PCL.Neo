# ModInfoReader 使用说明

## 简介
`ModInfoReader` 是一个用于读取 Minecraft 模组（mod）信息的工具类。它支持从指定的模组文件夹中批量解析 `.jar` 文件，自动识别并提取模组的名称、描述、版本、图标等信息，兼容 Fabric 和 Forge 两种主流模组格式。

## 使用方法
1. 引用命名空间：
```csharp
using PCL.Neo.Core.Models.Minecraft.Mod;
```
2. 调用静态方法 `GetModInfo`，传入模组文件夹路径：
```csharp
var mods = await ModInfoReader.GetModInfo("你的mods文件夹路径");
```
3. 遍历返回的模组信息：
```csharp
foreach (var modInfo in mods)
{
    Console.WriteLine($"名称: {modInfo.Name}");
    Console.WriteLine($"描述: {modInfo.Description}");
    Console.WriteLine($"版本: {modInfo.Version}");
    Console.WriteLine($"图标路径: {modInfo.Icon}");
    Console.WriteLine($"主页: {modInfo.Url}");
}
```

## 示例
```csharp
const string modDir = @"C:\你的Minecraft路径\mods";
var mods = await ModInfoReader.GetModInfo(modDir);
foreach (var modInfo in mods)
{
    Console.WriteLine(modInfo.Name);
    Console.WriteLine(modInfo.Description);
    Console.WriteLine(modInfo.Version);
    Console.WriteLine(modInfo.Icon);
    Console.WriteLine("--------");
}
```

## 注意事项
- 传入的文件夹路径必须存在且包含 `.jar` 格式的模组文件，否则会抛出异常。
- 仅支持 Fabric（`fabric.mod.json`）和 Forge（`META-INF/mods.toml`）格式的模组，其他格式会返回默认信息。
- 解析出的图标会被复制到本地缓存目录（`modIcons`），请注意及时清理无用图标文件。
- `ModInfo` 实现了 `IDisposable`，如需释放资源（删除图标文件），请在使用完毕后调用 `Dispose()` 方法。
- 该方法为异步方法，需使用 `await` 调用。

## 联系与反馈
如有问题或建议，欢迎在项目仓库提交 issue。
