namespace PCL.Neo.Core.Models.Configuration.Data;

public enum LauncherVisibleType
{
    None, // 无动作
    CLose, // 启动后关闭
    HideThenClose, // 启动后隐藏，游戏退出后关闭
    HideThenOpen, // 启动后隐藏，游戏退出后显示
    Hide // 启动后隐藏
}
