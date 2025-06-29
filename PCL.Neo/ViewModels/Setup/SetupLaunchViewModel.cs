using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace PCL.Neo.ViewModels.Setup;

public record JavaUiInfo(JavaRuntime Runtime)
{
    public string Identifier =>
        $"{(Runtime.IsJre ? "JRE" : "JDK")} {Runtime.SlugVersion} ({Runtime.Version}) {Runtime.Architecture} {Runtime.Implementor}";

    public string Path => Runtime.DirectoryPath;
}

[SubViewModel(typeof(SetupViewModel))]
public partial class SetupLaunchViewModel : ViewModelBase
{
    private readonly IJavaManager _javaManager;
    private readonly StorageService _storageService;
    [ObservableProperty] private ObservableCollection<JavaUiInfo> _javaInfoList = [];

    private void DoUiRefresh()
    {
        if (JavaInfoList.Count != 0) JavaInfoList.Clear();
        foreach (JavaRuntime runtime in _javaManager.JavaList)
            JavaInfoList.Add(new JavaUiInfo(runtime));
    }

    public SetupLaunchViewModel(IJavaManager javaManager, StorageService storageService)
    {
        _javaManager = javaManager;
        _storageService = storageService;
        DoUiRefresh();
    }

    [RelayCommand]
    private async Task RefreshJava()
    {
        JavaInfoList.Clear();
        await _javaManager.Refresh();
        DoUiRefresh();
    }

    [RelayCommand]
    private async Task ManualAdd()
    {
        string? javaPath = await _storageService.SelectFile("选择要添加的Java");
        if (javaPath == null) return;
        var dirPath = Path.GetDirectoryName(javaPath);
        if (dirPath == null) return;
        (JavaRuntime? resultRuntime, bool updateCurrent) = await _javaManager.ManualAdd(dirPath);
        if (resultRuntime == null || updateCurrent) return;
        JavaInfoList.Add(new JavaUiInfo(resultRuntime));
    }
}