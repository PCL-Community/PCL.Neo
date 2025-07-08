using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System.Timers;

namespace PCL.Neo.Views;

public partial class JobView : UserControl
{
    public Timer? InfoUpdateTimer;

    public JobView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (InfoUpdateTimer is null)
        {
            InfoUpdateTimer = new Timer();
            InfoUpdateTimer.Interval = 250;
            InfoUpdateTimer.AutoReset = true;
            InfoUpdateTimer.Elapsed += (_, _) =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    BindingOperations.GetBindingExpressionBase(ProgressText, TextBlock.TextProperty)?.UpdateTarget();
                });
            };
            InfoUpdateTimer.Enabled = true;
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        if (InfoUpdateTimer is not null)
        {
            InfoUpdateTimer.Enabled = false;
            InfoUpdateTimer.Dispose();
            InfoUpdateTimer = null;
        }
    }
}
