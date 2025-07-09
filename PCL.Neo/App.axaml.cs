using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using PCL.Neo.Core.Models.Minecraft.Game;
using PCL.Neo.Core.Models.Minecraft.Java;
using PCL.Neo.ViewModels;
using PCL.Neo.Views;
using PCL.Neo.Core.Service.Accounts;
using PCL.Neo.Core.Service.Accounts.MicrosoftAuth;
using PCL.Neo.Services;
using PCL.Neo.ViewModels;
using PCL.Neo.ViewModels.Download;
using PCL.Neo.ViewModels.Home;
using PCL.Neo.ViewModels.Job;
using PCL.Neo.ViewModels.Setup;
using PCL.Neo.Views;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PCL.Neo;

public class App : Application
{
    // public static Java? JavaManager { get; private set; }
    // public static IStorageProvider StorageProvider { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private static IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddTransient<MainWindowViewModel>()
            .AddTransient<HomeViewModel>()
            .AddTransient<HomeSubViewModel>()
            .AddTransient<VersionManagerViewModel>()
            .AddTransient<GameSettingsViewModel>()
            .AddTransient<DownloadViewModel>()
            .AddTransient<DownloadGameViewModel>()
            .AddTransient<DownloadModViewModel>()
            .AddTransient<LogViewModel>()
            .AddTransient<SetupViewModel>()
            .AddTransient<SetupLaunchViewModel>()
            .AddTransient<JobViewModel>()
            .AddTransient<JobSubViewModel>()
            .AddSingleton<INavigationService, NavigationService>()
            .AddSingleton<StorageService>()
            .AddSingleton<UserService>()
            .AddSingleton<JobService>()
            .AddSingleton<IAccountService, AccountService>()
            .AddSingleton<IJavaManager, JavaManager>()
            .AddSingleton<IGameService, GameService>()
            .AddSingleton<IGameLauncherService, GameLauncherService>()
            .AddSingleton<IMicrosoftAuthService, MicrosoftAuthService>()
            .BuildServiceProvider();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Ioc.Default.ConfigureServices(ConfigureServices());

        var vm = Ioc.Default.GetRequiredService<MainWindowViewModel>();
        Task.Run(Ioc.Default.GetRequiredService<IJavaManager>().JavaListInitAsync);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow { DataContext = vm };
            // 由于导航改成了异步方法，在构造函数中无法正常导向首页，需要在此处导向
            Ioc.Default.GetRequiredService<INavigationService>().GoToAsync<HomeViewModel>()
                .Wait(); // TODO: idk if this is appropriate
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
