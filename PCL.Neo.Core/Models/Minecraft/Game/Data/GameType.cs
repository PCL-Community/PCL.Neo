namespace PCL.Neo.Core.Models.Minecraft.Game.Data;

using System;

[Flags]
public enum GameType
{
    // --- 零值 ---
    // 代表未定义或无任何类型。
    Unknown = 0,

    // --- 组 1: 基础游戏类型 (这些值互相排斥) ---
    // 使用低位比特。
    Vanilla = 1 << 0, // 0x00000001
    Snapshot = 1 << 1, // 0x00000002
    Forge = 1 << 2, // 0x00000004
    Fabric = 1 << 3, // 0x00000008
    NeoForge = 1 << 4, // 0x00000010
    LiteLoader = 1 << 5, // 0x00000020
    Rift = 1 << 6, // 0x00000040
    Quit = 1 << 7, // 0x00000080
    Cleanroom = 1 << 8, // 0x000000010000000
    Labymod = 1 << 9, //   0x000000100000000
    Release = 1 << 10, // 0x00000100

    // --- 组 2: 修饰符 (可以与其他值组合) ---
    // 使用高位比特，为未来扩展预留空间。
    Optifine = 1 << 16, // 0b000000010000000000000000
    Fool = 1 << 17, //     0b000000100000000000000000

    // --- 逻辑掩码 (用于验证和判断) ---

    /// <summary>
    /// 包含所有基础游戏类型的掩码。
    /// 在一个有效的 GameType 值中，与此掩码进行位与操作后，结果必须是0或者一个2的幂。
    /// </summary>
    BaseTypeMask = Vanilla | Snapshot | Forge | Fabric | NeoForge | LiteLoader | Rift | Quit | Cleanroom | Labymod,

    ///// <summary>
    ///// 包含与 'Fool' 修饰符不兼容的所有基础类型的掩码。
    ///// </summary>
    //FoolIncompatibleMask = Release
}