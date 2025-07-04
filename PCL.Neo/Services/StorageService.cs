using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.Services;

public class StorageService
{
    /// <summary>
    /// 弹出一个系统的文件选择框给用户手动导出/选择文件路径用的
    /// </summary>
    private IStorageProvider? StorageProvider { get; } =
        (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
        ?.MainWindow?.StorageProvider;

    /// <summary>
    /// 打开系统文件选择框选择一个文件
    /// </summary>
    /// <param name="title">文件选择框的标题</param>
    /// <param name="filters">选择文件的要求</param>
    /// <returns>获得文件的路径</returns>
    public async Task<string?> SelectFile(string title = "选择文件", IReadOnlyList<FilePickerFileType>? filters = null)
    {
        if (StorageProvider == null) throw new NullReferenceException(nameof(StorageProvider));
        if (!StorageProvider.CanOpen) throw new InvalidOperationException(nameof(StorageProvider));
        var files = await StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = title, AllowMultiple = false });
        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }

    /// <summary>
    /// 打开系统文件夹选择框选择一个文件夹
    /// </summary>
    /// <param name="title">文件夹选择框的标题</param>
    /// <returns>获得文件夹的路径</returns>
    public async Task<string?> SelectFolder(string title = "选择文件夹")
    {
        if (StorageProvider == null) throw new NullReferenceException(nameof(StorageProvider));
        if (!StorageProvider.CanPickFolder) throw new InvalidOperationException("无法打开文件夹选择对话框");

        var folders = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = title, AllowMultiple = false });

        if (folders.Count < 1)
            return null;

        var folder = folders[0];
        return folder.Path.LocalPath;
    }

    /// <summary>
    /// 打开系统文件保存对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="fileName">默认文件名</param>
    /// <param name="extension">文件扩展名</param>
    /// <returns>保存文件的路径</returns>
    public async Task<string?> SaveFile(string title, string fileName, string extension)
    {
        if (StorageProvider == null) throw new NullReferenceException(nameof(StorageProvider));
        if (!StorageProvider.CanSave) throw new InvalidOperationException("无法打开文件保存对话框");

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = fileName + extension,
            DefaultExtension = extension,
            FileTypeChoices = new[]
            {
                new FilePickerFileType(extension)
                {
                    Patterns = new[] { "*" + extension }, MimeTypes = new[] { "application/octet-stream" }
                }
            }
        });
        return file?.Path.LocalPath;
    }

    /// <summary>
    /// 检查是否拥有某一文件夹的 I/O 权限。如果文件夹不存在，会返回 False。
    /// </summary>
    /// <param name="path">文件夹路径</param>
    /// <returns></returns>
    public bool CheckPermission(string path)
    {
        var file = StorageProvider?.TryGetFolderFromPathAsync(path);
        var result = file?.GetAwaiter().GetResult();
        return result != null;
    }

    /// <summary>
    /// 获取应用数据目录
    /// </summary>
    public static string AppDataDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PCL.Neo");
}
