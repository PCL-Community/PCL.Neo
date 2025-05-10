using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PCL.Neo.Controls.MyMsg;
using PCL.Neo.Services;
using PCL.Neo.Helpers;
using System.Threading.Tasks;


namespace PCL.Neo.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private Window? _window;
        public NavigationService NavigationService { get; }

        // quite shitty, maybe consider using enum and converters
        [ObservableProperty] private bool _isNavBtn1Checked = true;
        [ObservableProperty] private bool _isNavBtn2Checked;
        [ObservableProperty] private bool _isNavBtn3Checked;
        [ObservableProperty] private bool _isNavBtn4Checked;
        [ObservableProperty] private bool _isNavBtn5Checked;

        [ObservableProperty]
        private ViewModelBase? _currentViewModel;
        [ObservableProperty]
        private ViewModelBase? _currentSubViewModel;
        
        [ObservableProperty]
        private bool _canGoBack;

        // 为了设计时的 DataContext
        public MainWindowViewModel()
        {
            throw new System.NotImplementedException();
        }
        public MainWindowViewModel(Window window)
        {
            this._window = window;
        }
        public MainWindowViewModel(NavigationService navigationService)
        {
            this.NavigationService = navigationService;
            this.NavigationService.CurrentViewModelChanged += x =>
            {
                CurrentViewModel = x;
                CanGoBack = NavigationService.CanGoBack;
                switch (x)
                {
                    case HomeViewModel:
                        IsNavBtn1Checked = true;
                        IsNavBtn2Checked = false;
                        IsNavBtn3Checked = false;
                        IsNavBtn4Checked = false;
                        IsNavBtn5Checked = false;
                        break;
                    case DownloadViewModel:
                        IsNavBtn1Checked = false;
                        IsNavBtn2Checked = true;
                        IsNavBtn3Checked = false;
                        IsNavBtn4Checked = false;
                        IsNavBtn5Checked = false;
                        break;
                }
            };
            this.NavigationService.CurrentSubViewModelChanged += x =>
            {
                CurrentSubViewModel = x;
                CanGoBack = NavigationService.CanGoBack;
            };
            this.NavigationService.Goto<HomeViewModel>();
        }

        [RelayCommand]
        public void NavigateToHome()
        {
            this.NavigationService.Goto<HomeViewModel>();
        }

        [RelayCommand]
        public void NavigateToDownload()
        {
            this.NavigationService.Goto<DownloadViewModel>();
        }
        
        [RelayCommand]
        public void GoBack()
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                CanGoBack = NavigationService.CanGoBack;
            }
        }

        public void Close()
        {
            _window?.Close();
        }

        public void Minimize()
        {
            if (_window is null) return;
            _window.WindowState = WindowState.Minimized;
        }

        public void ShowMessageBox((MessageBoxParam, TaskCompletionSource<MessageBoxResult>) messageBox)
        {

        }

        /// <summary>
        /// 强制关闭正在窗口上展示的 MessageBox。
        /// </summary>
        public void CloseMessageBox()
        {

        }
    }
}
