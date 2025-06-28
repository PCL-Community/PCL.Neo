# PCL.Neo 贡献指南与开发规范

## 项目概述

PCL.Neo 是一个基于 .NET 9 和 Avalonia 框架开发的 Minecraft 启动器项目。项目采用分层架构设计，包含以下主要模块：

- **PCL.Neo**: UI 层，基于 Avalonia 框架的用户界面
- **PCL.Neo.Core**: 核心业务逻辑层，包含游戏启动、版本管理等核心功能
- **PCL.Neo.Tests**: 单元测试项目，确保代码质量和稳定性

## 开发环境要求

### 系统要求
- .NET 9 SDK 或更高版本
- Visual Studio 2022 或 JetBrains Rider 或 VS Code
- Git 版本控制工具

### 推荐工具
- GitHub Desktop 或其他 Git 客户端
- NuGet Package Manager
- 代码分析工具（如 SonarLint）

## 构建指令

### 命令行构建

项目支持通过命令行进行构建，以下是标准的构建命令：

#### Debug 构建
```bash
# 恢复 NuGet 包
dotnet restore

# Debug 构建（单文件发布，关闭代码裁剪）
dotnet publish PCL.Neo/PCL.Neo.csproj -c Debug -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/debug

# 运行测试
dotnet test PCL.Neo.Tests/
```

#### Release 构建
```bash
# 恢复 NuGet 包
dotnet restore

# Release 构建（单文件发布，关闭代码裁剪）
dotnet publish PCL.Neo/PCL.Neo.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/release

# 运行测试
dotnet test PCL.Neo.Tests/ -c Release
```

#### 跨平台构建
```bash
# Windows x64
dotnet publish PCL.Neo/PCL.Neo.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/win-x64

# Linux x64
dotnet publish PCL.Neo/PCL.Neo.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/linux-x64

# macOS x64
dotnet publish PCL.Neo/PCL.Neo.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o ./publish/osx-x64
```

### 构建要求
- **单文件发布**: 使用 `PublishSingleFile=true` 将应用程序打包为单个可执行文件
- **关闭代码裁剪**: 使用 `PublishTrimmed=false` 确保所有依赖项都包含在内，避免运行时缺少程序集
- **自包含部署**: 使用 `--self-contained true` 包含 .NET 运行时，无需目标机器安装 .NET

### 验证构建
构建完成后，请验证：
- [ ] 可执行文件能够正常启动
- [ ] 所有功能正常工作
- [ ] 没有缺少依赖项的错误
- [ ] 单元测试全部通过

## 项目结构

```
PCL.Neo/
├── PCL.Neo/                    # UI 层 - Avalonia 界面项目
├── PCL.Neo.Core/              # 核心层 - 业务逻辑和数据处理
│   ├── Models/                # 数据模型
│   ├── Services/              # 业务服务
│   ├── Utils/                 # 工具类
│   └── ...
├── PCL.Neo.Tests/             # 测试项目
├── docs/                      # 文档
└── README.md
```

## 代码规范

### 命名约定

遵循 Microsoft C# 命名约定：

- **类名**: PascalCase（如 `GameLauncher`）
- **方法名**: PascalCase（如 `LaunchAsync`）
- **属性名**: PascalCase（如 `RootDirectory`）
- **字段名**: camelCase，私有字段使用下划线前缀（如 `_isIndie`）
- **常量**: PascalCase（如 `DefaultTimeout`）
- **接口**: 以 `I` 开头的 PascalCase（如 `IGameLauncher`）

### 代码风格

项目使用 EditorConfig 统一代码风格，主要规范包括：

- **缩进**: 4 个空格，不使用制表符
- **换行**: 使用 CRLF（Windows 风格）
- **括号**: 所有控制结构都使用大括号，大括号另起一行
- **空格**: 在操作符前后添加空格，逗号后添加空格
- **文件编码**: UTF-8

### 代码质量要求

#### 1. 异常处理
如果需要忽略异常请使用`try-catch`块捕获后在需要忽略的部分使用注释说明
不要随意捕获异常，没有必要的捕获请删除
如果需要日志记录请使用`Utils`中的`Logger`，并将异常也传入`Logger`的参数中
```csharp
// 好的示例
public async Task<Process> LaunchAsync(GameProfile profile)
{
    if (profile == null)
        throw new ArgumentNullException(nameof(profile));

    string mcDir = profile.Information.RootDirectory;
    if (!Directory.Exists(mcDir))
    {
        throw new DirectoryNotFoundException($"Minecraft root directory not found. {mcDir}");
    }

    // ... 其他逻辑
}

public int Foo()
{
    try
    {
        // do stuff...
    }
    catch (IOException ex)
    {
        Logger.Error("Catched exception.", ex);
        // do stuff...
    }
    catch (Exception)
    {
        // 忽略异常
    }
}
```

#### 2. 空值检查
将所有的有关空值的Warning视为Error，正确地处理空值，谨防`NullRefreenceEexception`
```csharp
// 使用 null 条件运算符和 ArgumentNullException.ThrowIfNull
ArgumentNullException.ThrowIfNull(versionManifes.Libraries);

// 使用 null 合并运算符
var assetIndex = versionManifes.AssetIndex?.Id ?? "legacy";
```

#### 3. 资源管理
应使用`IDisposable`进行资源释放，尽量减少`Finalizer`的使用
```csharp
// 使用 using 语句确保资源释放
using var fileStream = new FileStream(path, FileMode.Open);
// 或者
using (var process = new Process())
{
    // 处理逻辑
}
```

#### 4. 异步编程
```csharp
// 正确使用 async/await
public async Task<VersionManifes> GetVersionAsync(string versionId)
{
    var result = await SomeAsyncOperation();
    return result;
}

// 避免异步方法的阻塞调用
// 错误: SomeAsyncMethod().Result
// 正确: await SomeAsyncMethod()
```

### 注释规范
对于注释请使用`//+空格+内容`的形式
#### 1. XML 文档注释
对于公开的方法请添加XML注释以便于后续使用，如果方法有抛出异常请使用`exception`块说明
```csharp
/// <summary>
/// 启动游戏
/// </summary>
/// <param name="profile">游戏配置文件</param>
/// <returns>游戏进程</returns>
/// <exception cref="DirectoryNotFoundException">当游戏目录不存在时抛出</exception>
public async Task<Process> LaunchAsync(GameProfile profile)
{
    // 实现逻辑
}
```

#### 2. 行内注释
如果是为完成的部分请添加`// TODO`块，便于IDE智能查找
```csharp
// 确保目录存在
if (!Directory.Exists(mcDir))
{
    throw new DirectoryNotFoundException($"Minecraft root directory not found. {mcDir}");
}

// TODO: 从配置文件加载版本信息
var launcherVersion = "1.0.0";
```

## 测试规范
测试使用NUnit 4.3.2框架
PCL.Neo.Core中的测试代码请放到Core文件夹中
### 单元测试要求

1. **测试覆盖率**: 核心业务逻辑测试覆盖率应达到 80% 以上
2. **测试命名**: 使用 `方法名_测试场景_期望结果` 格式
3. **测试结构**: 使用 AAA 模式（Arrange, Act, Assert）

```csharp
[Test]
public async Task LaunchAsync_ValidProfile_ReturnsProcess()
{
    // Arrange
    var profile = new GameProfile
    {
        Information = new GameInfo
        {
            RootDirectory = @"C:\Test\.minecraft",
            GameDirectory = @"C:\Test\.minecraft"
        }
    };
    var launcher = new GameLauncher();

    // Act
    var result = await launcher.LaunchAsync(profile);

    // Assert
    Assert.IsNotNull(result);
    Assert.IsInstanceOf<Process>(result);
}
```

### 测试分类

- **单元测试**: 测试单个方法或类的功能
- **集成测试**: 测试多个组件之间的交互
- **UI 测试**: 测试用户界面功能（使用 Avalonia 测试框架）

## Git 工作流程

### 分支策略

采用 Git Flow 分支模型：

- **main**: 主分支，包含生产就绪的代码
- **develop**: 开发分支，包含下一个发布版本的功能
- **feature/**: 功能分支，用于开发新功能
- **hotfix/**: 热修复分支，用于紧急修复
- **release/**: 发布分支，用于准备新版本发布

### 提交规范

使用语义化提交信息格式：

```
<类型>[可选的作用域]: <描述>

[可选的正文]

[可选的脚注]
```

**类型说明**:
- `feat`: 新功能
- `fix`: 修复 bug
- `docs`: 文档更新
- `style`: 代码格式调整（不影响功能）
- `refactor`: 代码重构
- `test`: 添加或修改测试
- `chore`: 构建过程或辅助工具的变动

**示例**:
```
feat(launcher): 添加游戏启动前的版本检查功能

- 添加版本兼容性检查
- 优化启动参数构建逻辑
- 增加错误处理和用户提示

Closes #123
```

## Pull Request 规范

### PR 标题格式
```
[类型] 简短描述
```

### PR 描述模板

```markdown
# 前言
简要描述这个 PR 的目的和背景。

# 引入的 NuGet 包
如果引入了新的 NuGet 包，请列出：
- 包名：简要说明用途
- 包名：简要说明用途

# 更改内容
详细列出主要更改：
- 添加了 XXX 功能
- 修复了 XXX 问题
- 重构了 XXX 模块
- 优化了 XXX 性能

# 使用说明
如果涉及 API 更改或新功能，请提供使用示例：

```csharp
// 示例代码
var launcher = new GameLauncher();
var process = await launcher.LaunchAsync(profile);
```

# 测试
说明如何测试这些更改：
- [ ] 单元测试已通过
- [ ] 集成测试已通过
- [ ] 手动测试已完成

# 破坏性更改
如果有破坏性更改，请详细说明：
- 更改的 API
- 迁移指南
- 影响范围

# 相关 Issue
关闭或关联的 Issue：
- Closes #123
- Relates to #456

```
最后，感谢你对 PCL.Neo 项目的贡献！欢迎多多提交PR！
