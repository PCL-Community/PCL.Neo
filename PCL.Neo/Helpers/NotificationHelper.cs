using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using PCL.Neo.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PCL.Neo.Helpers;

public class MessageBoxParam
{
    public string? Message;
    public string Title = "提示";
    public string Button1Text = "确定";
    public string? Button2Text;
    public string? Button3Text;
    public Action<int>? ButtonPressedAction;
    public MessageBoxType Type = MessageBoxType.Notification;
    public bool IsCloseWhenButton1Pressed;
    public bool IsCloseWhenButton2Pressed;
    public bool IsCloseWhenButton3Pressed;
}

public enum MessageBoxType
{
    Notification,
    Warning,
    Error
}

public class MessageBoxResult
{
    public bool Button1Clicked;
    public bool Button2Clicked;
    public bool Button3Clicked;
    public bool Button1ClickedBeforeClose;
    public bool Button2ClickedBeforeClose;
    public bool Button3ClickedBeforeClose;
}

public static class NotificationHelper
{
    private static ConcurrentQueue<(MessageBoxParam, TaskCompletionSource<MessageBoxResult>)> _messageBoxQueue = new();
    private static SemaphoreSlim _messageBoxSemaphore = new(1, 1);

    #region MessageBox 重载

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(string message)
    {
        return await ShowMessageBoxAsync(message, "提示");
    }

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <param name="title">标题。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(string message, string title = "提示")
    {
        return await ShowMessageBoxAsync(
            message,
            title,
            "确定",
            null,
            null);
    }

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <param name="title">标题。</param>
    /// <param name="button1Text">第一个按钮要展示的内容。</param>
    /// <param name="button2Text">第二个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="button3Text">第三个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string title = "提示",
        string button1Text = "确定",
        string? button2Text = null,
        string? button3Text = null)
    {
        return await ShowMessageBoxAsync(
            message,
            title,
            button1Text,
            button2Text,
            button3Text,
            MessageBoxType.Notification);
    }

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <param name="title">标题。</param>
    /// <param name="button1Text">第一个按钮要展示的内容。</param>
    /// <param name="button2Text">第二个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="button3Text">第三个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="type">MessageBox 的类型。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string title = "提示",
        string button1Text = "确定",
        string? button2Text = null,
        string? button3Text = null,
        MessageBoxType type = MessageBoxType.Notification)
    {
        return await ShowMessageBoxAsync(
            message,
            title,
            button1Text,
            button2Text,
            button3Text,
            null,
            type);
    }

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <param name="title">标题。</param>
    /// <param name="button1Text">第一个按钮要展示的内容。</param>
    /// <param name="button2Text">第二个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="button3Text">第三个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="buttonPressedAction">按钮按下时执行的 Action。返回 int 值代表第几个按钮。</param>
    /// <param name="type">MessageBox 的类型。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string title = "提示",
        string button1Text = "确定",
        string? button2Text = null,
        string? button3Text = null,
        Action<int>? buttonPressedAction = null,
        MessageBoxType type = MessageBoxType.Notification)
    {
        return await ShowMessageBoxAsync(
            message,
            title,
            button1Text,
            button2Text,
            button3Text,
            buttonPressedAction,
            type,
            true);
    }

    #endregion

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="message">展示的内容。</param>
    /// <param name="title">标题。</param>
    /// <param name="button1Text">第一个按钮要展示的内容。</param>
    /// <param name="button2Text">第二个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="button3Text">第三个按钮要展示的内容。如果为空，那么没有此按钮。</param>
    /// <param name="buttonPressedAction">按钮按下时执行的 Action。返回 int 值代表第几个按钮。</param>
    /// <param name="type">MessageBox 的类型。</param>
    /// <param name="isCloseWhenButton1Pressed">按下第一个按钮时是否关闭 MessageBox。</param>
    /// <param name="isCloseWhenButton2Pressed">按下第二个按钮时是否关闭 MessageBox。</param>
    /// <param name="isCloseWhenButton3Pressed">按下第三个按钮时是否关闭 MessageBox。</param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxAsync(
        string message,
        string title = "提示",
        string button1Text = "确定",
        string? button2Text = null,
        string? button3Text = null,
        Action<int>? buttonPressedAction = null,
        MessageBoxType type = MessageBoxType.Notification,
        bool isCloseWhenButton1Pressed = true,
        bool isCloseWhenButton2Pressed = true,
        bool isCloseWhenButton3Pressed = true)
    {
        return await ShowMessageBoxIndirectAsync(new MessageBoxParam
        {
            Message = message,
            Title = title,
            Button1Text = button1Text,
            Button2Text = button2Text,
            Button3Text = button3Text,
            ButtonPressedAction = buttonPressedAction,
            Type = type,
            IsCloseWhenButton1Pressed = isCloseWhenButton1Pressed,
            IsCloseWhenButton2Pressed = isCloseWhenButton2Pressed,
            IsCloseWhenButton3Pressed = isCloseWhenButton3Pressed
        });
    }

    /// <summary>
    /// 在主窗口上展示一个 MessageBox。
    /// </summary>
    /// <param name="param"></param>
    /// <returns>返回 MessageBoxResult，代表第几个按钮。</returns>
    public static async Task<MessageBoxResult> ShowMessageBoxIndirectAsync(MessageBoxParam param)
    {
        var tcs = new TaskCompletionSource<MessageBoxResult>();
        _messageBoxQueue.Enqueue((param, tcs));


        return new MessageBoxResult();
    }

    private static async Task ProcessMessageBoxQueueAsync()
    {
        await _messageBoxSemaphore.WaitAsync();

        try
        {
            // 取出每个 MessageBox 并展示
            while (_messageBoxQueue.TryDequeue(out var messageBox))
            {
                if (Application.Current is not null &&
                    Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    ((MainWindowViewModel)desktop.MainWindow!.DataContext!).ShowMessageBox((messageBox.Item1,
                        messageBox.Item2));
                }
            }
        }
        finally
        {
            _messageBoxSemaphore.Release();
        }
    }
}
