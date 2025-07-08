using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Linq;
using System.Timers;

namespace PCL.Neo.Views.Job;

public partial class JobSubView : UserControl
{
    public Timer? InfoUpdateTimer;

    public JobSubView()
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
                    foreach (var tb in this
                                 .GetVisualDescendants()
                                 .OfType<TextBlock>()
                                 .Where(x => x.Classes.Contains("update_me")))
                    {
                        BindingOperations.GetBindingExpressionBase(tb, TextBlock.TextProperty)?.UpdateTarget();
                    }
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
