using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PCL.Neo.ViewModels.Home;
using System.Windows.Input;

namespace PCL.Neo.Views.Home;

public partial class VersionManagerView : UserControl
{
    public VersionManagerView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // 代理命令属性
    public ICommand? RemoveDirectoryCommand
    {
        get
        {
            if (DataContext is VersionManagerViewModel vm)
            {
                return vm.RemoveDirectoryCommandCommand;
            }

            return null;
        }
    }

    public ICommand? LaunchVersionCommand
    {
        get
        {
            if (DataContext is VersionManagerViewModel vm)
            {
                return vm.LaunchVersionCommandCommand;
            }

            return null;
        }
    }

    public ICommand? SettingsCommand
    {
        get
        {
            if (DataContext is VersionManagerViewModel vm)
            {
                return vm.SettingsCommandCommand;
            }

            return null;
        }
    }

    public ICommand? DeleteVersionCommand
    {
        get
        {
            if (DataContext is VersionManagerViewModel vm)
            {
                return vm.DeleteVersionCommandCommand;
            }

            return null;
        }
    }
}
