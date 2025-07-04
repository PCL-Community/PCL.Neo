using PCL.Neo.Models;
using System;
using System.Linq;

namespace PCL.Neo.Utils;

public static class MathUtils
{
    /// <summary>
    /// 2~65 进制的转换。
    /// </summary>
    public static string RadixConvert(string input, int fromRadix, int toRadix)
    {
        const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        if (string.IsNullOrEmpty(input)) return "0";
        var isNegative = input.StartsWith("-");
        if (isNegative) input = input.TrimStart('-');
        long realNum = 0, scale = 1;

        foreach (var digit in input.Reverse().Select(l => digits.IndexOf(l.ToString())))
        {
            realNum += digit * scale;
            scale *= fromRadix;
        }

        var result = string.Empty;
        while (realNum > 0)
        {
            var newNum = (int)(realNum % toRadix);
            realNum = (realNum - newNum) / toRadix;
            result = digits[newNum] + result;
        }

        return (isNegative ? "-" : string.Empty) + result;
    }

    /// <summary>
    /// 计算二阶贝塞尔曲线。
    /// </summary>
    public static double MathBezier(double x, double x1, double y1, double x2, double y2, double acc = 0.01)
    {
        if (x <= 0 || double.IsNaN(x)) return 0;
        if (x >= 1) return 1;
        var a = x;
        double b;
        do
        {
            b = 3 * a * ((0.33333333 + x1 - x2) * a * a + (x2 - 2 * x1) * a + x1);
            a += (x - b) * 0.5;
        } while (Math.Abs(b - x) < acc);

        return 3 * a * ((0.33333333 + y1 - y2) * a * a + (y2 - 2 * y1) * a + y1);
    }

    /// <summary>
    /// 将一个数字限制为 0~255 的 Byte 值。
    /// </summary>
    public static byte MathByte(double d)
    {
        if (d < 0) d = 0;
        if (d > 255) d = 255;
        return (byte)Math.Round(d);
    }

    /// <summary>
    /// 提供 MyColor 类型支持的 Math.Round。
    /// </summary>
    public static MyColor MathRound(MyColor col, int w = 0)
    {
        return new MyColor
        {
            A = (float)Math.Round(col.A, w),
            R = (float)Math.Round(col.R, w),
            G = (float)Math.Round(col.G, w),
            B = (float)Math.Round(col.B, w)
        };
    }

    /// <summary>
    /// 获取两数间的百分比。小数点精确到 6 位。
    /// </summary>
    public static double MathPercent(double valueA, double valueB, double percent)
    {
        return Math.Round(valueA * (1 - percent) + valueB * percent, 6); // 解决 Double 计算错误
    }

    /// <summary>
    /// 获取两颜色间的百分比，根据 RGB 计算。小数点精确到 6 位。
    /// </summary>
    public static MyColor MathPercent(MyColor valueA, MyColor valueB, double percent)
    {
        return MathRound(valueA * (1 - percent) + valueB * percent, 6); // 解决 Double 计算错误
    }

    /// <summary>
    /// 将数值限定在某个范围内。
    /// </summary>
    public static double MathClamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    /// <summary>
    /// 符号函数。
    /// </summary>
    public static int MathSgn(double value)
    {
        return value switch
        {
            0 => 0,
            > 0 => 1,
            _ => -1
        };
    }
}
