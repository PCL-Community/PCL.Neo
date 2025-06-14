using System;

namespace PCL.Neo.ViewModels.Game;

/// <summary>
/// 常规游戏版本的版本号，后续可能会拓展到模组版本
/// </summary>
public record GameVersionNum(byte Sub, byte? Fix = null) : IComparable<GameVersionNum>
{
    private readonly (byte Major, byte Sub, byte Fix) _version = (1, Sub, Fix ?? 0);

    public byte Major => _version.Major;
    public byte Sub => _version.Sub;
    public byte? Fix => _version.Fix > 0 ? _version.Fix : null;

    public int CompareTo(GameVersionNum? other) =>
        other == null ? 1 : (Major, Sub, Fix ?? 0).CompareTo((other.Major, other.Sub, other.Fix ?? 0));

    public override string ToString() =>
        Fix.HasValue ? $"{Major}.{Sub}.{Fix}" : $"{Major}.{Sub}";

    /// <inheritdoc />
    public override int GetHashCode() =>
        HashCode.Combine(_version.Major, _version.Sub, _version.Fix);

    /// <inheritdoc />
    public virtual bool Equals(GameVersionNum? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return _version.Major == other._version.Major &&
               _version.Sub == other._version.Sub &&
               _version.Fix == other._version.Fix;
    }
}