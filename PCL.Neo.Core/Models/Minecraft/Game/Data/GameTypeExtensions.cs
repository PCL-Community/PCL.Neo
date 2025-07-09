namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

internal static class GameTypeExtensions
{
    /// <summary>
    /// 检查给定的 GameType 组合是否有效。
    /// </summary>
    /// <param name="type">要检查的 GameType 值。</param>
    /// <returns>如果组合有效，则为 true；否则为 false。</returns>
    public static bool IsValid(this GameType type)
    {
        // 规则 1: 基础类型必须是唯一的或不存在。
        // 提取出值中的所有基础类型部分。
        var baseTypes = type & GameType.BaseTypeMask;

        // Bit-twiddling hack: 一个数如果是2的幂，那么 (x & (x - 1)) == 0。
        // 这个检查允许 `baseTypes` 为 0 (没有基础类型) 或只有一个基础类型被设置。
        // 如果设置了多个基础类型位，(baseTypes & (baseTypes - 1)) 将不为 0。
        if (baseTypes != 0 && (baseTypes & (baseTypes - 1)) != 0)
        {
            return false; // 错误：包含多个互斥的基础类型。
        }

        // 规则 2: 如果包含 Fool，则不能包含 或 Release。
        // 检查 'Fool' 标志是否存在。
        if (type.HasFlag(GameType.Fool))
        {
            // 检查是否同时包含了任何与 Fool 不兼容的类型。
            if ((type & GameType.Release) != 0)
            {
                return false; // 错误：Fool 与 Release 共存。
            }
        }

        // 所有规则都通过
        return true;
    }

    /// <summary>
    /// 从组合值中获取基础游戏类型。
    /// </summary>
    /// <param name="type">一个 GameType 组合值。</param>
    /// <returns>返回唯一的基础类型。如果没有或有多个，则返回 GameType.Unknown。</returns>
    public static GameType GetBaseType(this GameType type)
    {
        var baseTypes = type & GameType.BaseTypeMask;
        if (baseTypes == 0 || (baseTypes & (baseTypes - 1)) != 0)
        {
            return GameType.Unknown;
        }

        return baseTypes;
    }
}