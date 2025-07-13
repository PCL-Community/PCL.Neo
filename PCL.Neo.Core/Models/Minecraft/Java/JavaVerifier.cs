using PCL.Neo.Core.Utils;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace PCL.Neo.Core.Models.Minecraft.Java;

/// <summary>
/// Java验证器，用于检测Java的真伪和来源
/// </summary>
public static partial class JavaVerifier
{
    /// <summary>
    /// Java厂商枚举
    /// </summary>
    public enum JavaVendor
    {
        Unknown,
        Oracle,
        OpenJDK,
        AdoptOpenJDK,
        AdoptiumEclipse,
        Microsoft,
        Amazon,
        Azul,
        Alibaba,
        Tencent,
        BellSoft,
        SAP,
        RedHat
    }

    /// <summary>
    /// Java验证结果
    /// </summary>
    public record JavaVerifyResult
    {
        /// <summary>
        /// 是否为正版Java
        /// </summary>
        public bool IsGenuine { get; init; }

        /// <summary>
        /// 验证失败的原因
        /// </summary>
        public string? FailReason { get; init; }

        /// <summary>
        /// Java厂商
        /// </summary>
        public JavaVendor Vendor { get; init; } = JavaVendor.Unknown;

        /// <summary>
        /// 厂商描述
        /// </summary>
        public string? VendorDescription { get; init; }

        /// <summary>
        /// 构建号或标识
        /// </summary>
        public string? BuildIdentifier { get; init; }

        /// <summary>
        /// 是否为开发者预览版
        /// </summary>
        public bool IsEarlyAccess { get; init; }
    }

    /// <summary>
    /// 验证Java是否为正版并获取厂商信息
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>验证结果</returns>
    public static async Task<JavaVerifyResult> VerifyJavaAsync(string javaPath)
    {
        if (!File.Exists(javaPath))
        {
            return new JavaVerifyResult
            {
                IsGenuine = false,
                FailReason = "Java可执行文件不存在"
            };
        }

        // 1. 检查文件签名（Windows平台）
        if (SystemUtils.Os == SystemUtils.RunningOs.Windows)
        {
            var signatureResult = await VerifyDigitalSignatureAsync(javaPath);
            if (!signatureResult.IsGenuine)
            {
                return signatureResult;
            }
        }

        // 2. 运行版本检查
        try
        {
            var versionOutput = await GetJavaVersionOutputAsync(javaPath);
            var vendorInfo = ParseVendorInfo(versionOutput);

            // 3. 验证Java语言功能
            var languageTestResult = await VerifyJavaLanguageFeaturesAsync(javaPath);
            if (!languageTestResult)
            {
                return new JavaVerifyResult
                {
                    IsGenuine = false,
                    FailReason = "Java语言功能验证失败，可能是伪装的可执行文件",
                    Vendor = vendorInfo.Vendor,
                    VendorDescription = vendorInfo.VendorDescription
                };
            }

            return new JavaVerifyResult
            {
                IsGenuine = true,
                Vendor = vendorInfo.Vendor,
                VendorDescription = vendorInfo.VendorDescription,
                BuildIdentifier = vendorInfo.BuildIdentifier,
                IsEarlyAccess = vendorInfo.IsEarlyAccess
            };
        }
        catch (Exception ex)
        {
            return new JavaVerifyResult
            {
                IsGenuine = false,
                FailReason = $"验证过程中出现异常: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// 验证Windows下的数字签名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>验证结果</returns>
    private static async Task<JavaVerifyResult> VerifyDigitalSignatureAsync(string filePath)
    {
        if (SystemUtils.Os != SystemUtils.RunningOs.Windows)
        {
            return new JavaVerifyResult { IsGenuine = true }; // 非Windows平台暂不检查签名
        }

        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Get-AuthenticodeSignature '{filePath}' | Format-List\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // 检查签名状态
            var statusMatch = Regex.Match(output, @"Status\s*:\s*(\w+)");
            if (!statusMatch.Success || statusMatch.Groups[1].Value != "Valid")
            {
                return new JavaVerifyResult
                {
                    IsGenuine = false,
                    FailReason = "数字签名无效"
                };
            }

            // 检查签名者
            var signerMatch = JavaSignerCerificateChecker().Match(output);
            if (signerMatch.Success)
            {
                var signer = signerMatch.Groups[1].Value.Trim();
                var vendor = DetermineVendorFromSignature(signer);

                return new JavaVerifyResult
                {
                    IsGenuine = true,
                    Vendor = vendor,
                    VendorDescription = signer
                };
            }

            return new JavaVerifyResult { IsGenuine = true };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"验证签名异常: {ex.Message}");
            return new JavaVerifyResult { IsGenuine = true }; // 签名验证失败但不确定是否为假冒，继续后续检查
        }
    }

    /// <summary>
    /// 从签名信息确定厂商
    /// </summary>
    private static JavaVendor DetermineVendorFromSignature(string signature)
    {
        signature = signature.ToLowerInvariant();

        if (signature.Contains("oracle")) return JavaVendor.Oracle;
        if (signature.Contains("microsoft")) return JavaVendor.Microsoft;
        if (signature.Contains("eclipse") || signature.Contains("adoptium")) return JavaVendor.AdoptiumEclipse;
        if (signature.Contains("adopt") || signature.Contains("openjdk")) return JavaVendor.AdoptOpenJDK;
        if (signature.Contains("amazon")) return JavaVendor.Amazon;
        if (signature.Contains("azul")) return JavaVendor.Azul;
        if (signature.Contains("alibaba")) return JavaVendor.Alibaba;
        if (signature.Contains("tencent")) return JavaVendor.Tencent;
        if (signature.Contains("bellsoft") || signature.Contains("liberica")) return JavaVendor.BellSoft;
        if (signature.Contains("sap")) return JavaVendor.SAP;
        if (signature.Contains("redhat")) return JavaVendor.RedHat;

        return JavaVendor.Unknown;
    }

    /// <summary>
    /// 获取Java版本信息输出
    /// </summary>
    private static async Task<string> GetJavaVersionOutputAsync(string javaPath)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = javaPath,
            Arguments = "-XshowSettings:properties -version",
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        process.Start();
        // Java将版本信息输出到标准错误流
        var errorOutput = await process.StandardError.ReadToEndAsync();
        var standardOutput = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return errorOutput + standardOutput;
    }

    /// <summary>
    /// 解析厂商信息
    /// </summary>
    private static (JavaVendor Vendor, string? VendorDescription, string? BuildIdentifier, bool IsEarlyAccess)
        ParseVendorInfo(string versionOutput)
    {
        var vendor = JavaVendor.Unknown;
        string? vendorDescription = null;
        string? buildIdentifier = null;
        var isEarlyAccess = false;

        // 查找VM信息
        var vmMatch = Regex.Match(versionOutput, @"(?:JRE|JDK|Runtime Environment|OpenJDK).*\((.*)\)",
            RegexOptions.IgnoreCase);
        if (vmMatch.Success)
        {
            vendorDescription = vmMatch.Groups[1].Value.Trim();
        }

        // 查找具体版本和构建标识
        var buildMatch = Regex.Match(versionOutput, @"build\s+([^\s]+)");
        if (buildMatch.Success)
        {
            buildIdentifier = buildMatch.Groups[1].Value.Trim();
        }

        // 判断Early Access
        isEarlyAccess = versionOutput.Contains("Early-Access") ||
                        versionOutput.Contains("EA") ||
                        (buildIdentifier?.Contains("-ea") ?? false);

        // 确定厂商
        versionOutput = versionOutput.ToLowerInvariant();

        if (versionOutput.Contains("openjdk"))
        {
            vendor = JavaVendor.OpenJDK;

            // 进一步确认发行商
            if (versionOutput.Contains("adoptium", StringComparison.OrdinalIgnoreCase) ||
                versionOutput.Contains("eclipse", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.AdoptiumEclipse;
            else if (versionOutput.Contains("adoptopenjdk", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("adopt", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.AdoptOpenJDK;
            else if (versionOutput.Contains("microsoft", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("msft", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.Microsoft;
            else if (versionOutput.Contains("amazon", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("corretto", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.Amazon;
            else if (versionOutput.Contains("azul", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("zulu", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.Azul;
            else if (versionOutput.Contains("alibaba", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("dragonwell", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.Alibaba;
            else if (versionOutput.Contains("tencent", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("kona", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.Tencent;
            else if (versionOutput.Contains("bellsoft", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("liberica", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.BellSoft;
            else if (versionOutput.Contains("sap", StringComparison.OrdinalIgnoreCase) ||
                     versionOutput.Contains("sapmachine", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.SAP;
            else if (versionOutput.Contains("redhat", StringComparison.OrdinalIgnoreCase))
                vendor = JavaVendor.RedHat;
        }
        else if (versionOutput.Contains("oracle", StringComparison.OrdinalIgnoreCase) ||
                 versionOutput.Contains("java(tm)", StringComparison.OrdinalIgnoreCase))
        {
            vendor = JavaVendor.Oracle;
        }

        return (vendor, vendorDescription, buildIdentifier, isEarlyAccess);
    }

    /// <summary>
    /// 验证Java语言功能
    /// </summary>
    /// <param name="javaPath">Java可执行文件路径</param>
    /// <returns>是否通过验证</returns>
    private static async Task<bool> VerifyJavaLanguageFeaturesAsync(string javaPath)
    {
        // 创建一个简单的Java程序进行测试
        var tempDir = Path.Combine(Path.GetTempPath(), "JavaVerification_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var javaFilePath = Path.Combine(tempDir, "Test.java");
        const string javaCode = """

                                public class Test {
                                    public static void main(String[] args) {
                                        System.out.println("JavaVerificationSuccess");
                                    }
                                }

                                """;
        try
        {
            await File.WriteAllTextAsync(javaFilePath, javaCode);

            // 编译
            using (var compileProcess = new Process())
            {
                var javacPath = Path.Combine(Path.GetDirectoryName(javaPath)!,
                    "javac" + (SystemUtils.Os == SystemUtils.RunningOs.Windows ? ".exe" : ""));

                if (!File.Exists(javacPath))
                {
                    // 如果没有javac，尝试使用java -jar功能验证
                    return await VerifyWithJarExecutionAsync(javaPath);
                }

                compileProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = javacPath,
                    Arguments = $"\"{javaFilePath}\"",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir
                };

                compileProcess.Start();
                await compileProcess.WaitForExitAsync();

                if (compileProcess.ExitCode != 0)
                {
                    return false;
                }
            }

            // 运行
            using (var runProcess = new Process())
            {
                runProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = "Test",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir
                };

                runProcess.Start();
                var output = await runProcess.StandardOutput.ReadToEndAsync();
                await runProcess.WaitForExitAsync();

                return output.Trim() == "JavaVerificationSuccess";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"验证Java语言功能异常: {ex.Message}");
            return false;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch
            {
                // 忽略清理异常
            }
        }
    }

    /// <summary>
    /// 使用jar执行功能验证Java
    /// </summary>
    private static async Task<bool> VerifyWithJarExecutionAsync(string javaPath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "JavaVerification_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var manifestPath = Path.Combine(tempDir, "MANIFEST.MF");
        const string manifestContent = "Main-Class: TestJar\r\n\r\n";

        var classPath = Path.Combine(tempDir, "TestJar.class");

        try
        {
            // 直接使用预编译的class文件的base64编码
            var classBytes = Convert.FromBase64String(
                "yv66vgAAADQAHAoABgAOBwAPCgAFABAIABEKABIAEwcAFAcAFQEABjxpbml0PgEAAygpVgEABENvZGUB" +
                "AA9MaW5lTnVtYmVyVGFibGUBAAg8Y2xpbml0PgEABG1haW4BABYoW0xqYXZhL2xhbmcvU3RyaW5nOylW" +
                "AQAKU291cmNlRmlsZQEADFRlc3RKYXIuamF2YQwABwAIBwAWDAAXABgBABNKYXZhVmVyaWZpY2F0aW9u" +
                "VGVzdAcAGQwAGgAbAQAHVGVzdEphcgEAEGphdmEvbGFuZy9PYmplY3QBABBqYXZhL2xhbmcvU3lzdGVt" +
                "AQADb3V0AQAVTGphdmEvaW8vUHJpbnRTdHJlYW07AQATamF2YS9pby9QcmludFN0cmVhbQEAB3ByaW50" +
                "bG4BABUoTGphdmEvbGFuZy9TdHJpbmc7KVYAIQAFAAYAAAAAAAIAAQAHAAgAAQAJAAAAHQABAAEAAAAF" +
                "KrcAAbEAAAABAAoAAAAGAAEAAAABAAsACwAIAAEACQAAACUAAgAAAAAACbIAAnEAA7EAAAABAAoAAAAK" +
                "AAIAAAADAAhAAg==");

            await File.WriteAllTextAsync(manifestPath, manifestContent);
            await File.WriteAllBytesAsync(classPath, classBytes);

            // 创建jar文件
            var jarPath = Path.Combine(tempDir, "test.jar");
            using (var jarProcess = new Process())
            {
                var jarTool = Path.Combine(Path.GetDirectoryName(javaPath)!,
                    "jar" + (SystemUtils.Os == SystemUtils.RunningOs.Windows ? ".exe" : ""));

                // 如果没有jar工具，则认为不是完整JDK，但仍可能是有效JRE
                if (!File.Exists(jarTool))
                {
                    return true;
                }

                jarProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = jarTool,
                    Arguments = $"cvfm \"{jarPath}\" \"{manifestPath}\" TestJar.class",
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir
                };

                jarProcess.Start();
                await jarProcess.WaitForExitAsync();

                if (jarProcess.ExitCode != 0 || !File.Exists(jarPath))
                {
                    return true; // jar命令失败，但不能确定Java是否正版，假设为真
                }
            }

            // 运行jar
            using (var runProcess = new Process())
            {
                runProcess.StartInfo = new ProcessStartInfo
                {
                    FileName = javaPath,
                    Arguments = $"-jar \"{jarPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = tempDir
                };

                runProcess.Start();
                var output = await runProcess.StandardOutput.ReadToEndAsync();
                await runProcess.WaitForExitAsync();

                return output.Trim() == "JavaVerificationTest";
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch
            {
                // 忽略清理异常
            }
        }
    }

    /// <summary>
    /// 获取Java厂商的友好名称
    /// </summary>
    /// <param name="vendor">Java厂商枚举</param>
    /// <returns>友好名称</returns>
    public static string GetVendorFriendlyName(JavaVendor vendor)
    {
        return vendor switch
        {
            JavaVendor.Oracle => "Oracle",
            JavaVendor.OpenJDK => "OpenJDK",
            JavaVendor.AdoptOpenJDK => "AdoptOpenJDK",
            JavaVendor.AdoptiumEclipse => "Eclipse Adoptium",
            JavaVendor.Microsoft => "Microsoft",
            JavaVendor.Amazon => "Amazon Corretto",
            JavaVendor.Azul => "Azul Zulu",
            JavaVendor.Alibaba => "Alibaba Dragonwell",
            JavaVendor.Tencent => "Tencent Kona",
            JavaVendor.BellSoft => "BellSoft Liberica",
            JavaVendor.SAP => "SAP Machine",
            JavaVendor.RedHat => "RedHat OpenJDK",
            _ => "未知供应商"
        };
    }

    [GeneratedRegex(@"SignerCertificate.*Subject\s*:\s*(.+)$", RegexOptions.Multiline)]
    private static partial Regex JavaSignerCerificateChecker();
}
