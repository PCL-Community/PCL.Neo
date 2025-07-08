using Avalonia.Media;
using System;
using System.Numerics;

namespace PCL.Neo.Models;

public class MyColor : IEquatable<MyColor>
{
    private Vector4 _color;

    public float A
    {
        get => _color.X;
        set => _color.X = value;
    }

    public float R
    {
        get => _color.Y;
        set => _color.Y = value;
    }

    public float G
    {
        get => _color.Z;
        set => _color.Z = value;
    }

    public float B
    {
        get => _color.W;
        set => _color.W = value;
    }

    // 类型转换

    public static implicit operator MyColor(string str)
    {
        return new MyColor(str);
    }

    public static implicit operator MyColor(Color col)
    {
        return new MyColor(col);
    }

    public static implicit operator Color(MyColor conv)
    {
        return Color.FromArgb((byte)Math.Clamp(conv.A, 0, 255),
            (byte)Math.Clamp(conv.R, 0, 255),
            (byte)Math.Clamp(conv.G, 0, 255),
            (byte)Math.Clamp(conv.B, 0, 255));
    }

    public static implicit operator MyColor(SolidColorBrush bru)
    {
        return new MyColor(bru.Color);
    }

    public static implicit operator SolidColorBrush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Math.Clamp(conv.A, 0, 255),
            (byte)Math.Clamp(conv.R, 0, 255),
            (byte)Math.Clamp(conv.G, 0, 255),
            (byte)Math.Clamp(conv.B, 0, 255)));
    }

    public static implicit operator MyColor(Brush bru)
    {
        return new MyColor(bru);
    }

    public static implicit operator Brush(MyColor conv)
    {
        return new SolidColorBrush(Color.FromArgb((byte)Math.Clamp(conv.A, 0, 255),
            (byte)Math.Clamp(conv.R, 0, 255),
            (byte)Math.Clamp(conv.G, 0, 255),
            (byte)Math.Clamp(conv.B, 0, 255)));
    }

    // 颜色运算

    public static MyColor operator +(MyColor a, MyColor b)
    {
        return new MyColor { _color = a._color + b._color };
    }

    public static MyColor operator -(MyColor a, MyColor b)
    {
        return new MyColor { _color = a._color - b._color };
    }

    public static MyColor operator *(MyColor a, float b)
    {
        return new MyColor { _color = a._color * b };
    }

    public static MyColor operator /(MyColor a, float b)
    {
        return new MyColor { _color = a._color / b };
    }

    public static MyColor operator *(MyColor a, double b)
    {
        return a * (float)b;
    }

    public static MyColor operator /(MyColor a, double b)
    {
        return a / (float)b;
    }

    public static bool operator ==(MyColor a, MyColor b)
    {
        return a._color == b._color;
    }

    public static bool operator !=(MyColor a, MyColor b)
    {
        return a._color != b._color;
    }

    // 构造函数

    public MyColor()
    {
        _color = new Vector4(255f, 0f, 0f, 0f);
    }

    public MyColor(Color color)
    {
        _color = new Vector4(color.A, color.R, color.G, color.B);
    }

    public MyColor(string hex)
    {
        var color = Color.Parse(hex);
        _color = new Vector4(color.A, color.R, color.G, color.B);
    }

    public MyColor(float a, MyColor color)
    {
        _color = color._color with { X = a };
    }

    public MyColor(float r, float g, float b)
    {
        _color = new Vector4(255f, r, g, b);
    }

    public MyColor(float a, float r, float g, float b)
    {
        _color = new Vector4(a, r, g, b);
    }

    public MyColor(Brush brush)
    {
        var color = ((SolidColorBrush)brush).Color;
        _color = new Vector4(color.A, color.R, color.G, color.B);
    }

    public MyColor(SolidColorBrush brush)
    {
        var color = brush.Color;
        _color = new Vector4(color.A, color.R, color.G, color.B);
    }

    // IEquatable

    public bool Equals(MyColor? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _color.Equals(other._color);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((MyColor)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(A, R, G, B);
    }

    // HSL

    public static double Hue(double v1, double v2, double vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        if (vH < 0.16667) return v1 + (v2 - v1) * 6 * vH;
        if (vH < 0.5) return v2;
        if (vH < 0.66667) return v1 + (v2 - v1) * (4 - vH * 6);
        return v1;
    }

    public static MyColor FromHsl(double sH, double sS, double sL)
    {
        var color = new MyColor();
        if (sS == 0)
        {
            color.R = (float)(sL * 2.55);
            color.G = color.R;
            color.B = color.R;
        }
        else
        {
            var h = sH / 360;
            var s = sS / 100;
            var l = sL / 100;
            s = l < 0.5 ? s * l + l : s * (1.0 - l) + l;
            l = 2 * l - s;
            color.R = (float)(255 * Hue(l, s, h + 1 / 3.0));
            color.G = (float)(255 * Hue(l, s, h));
            color.B = (float)(255 * Hue(l, s, h - 1 / 3.0));
        }

        color.A = 255;
        return color;
    }

    public static MyColor FromHsl2(double sH, double sS, double sL)
    {
        var color = new MyColor();
        if (sS == 0)
        {
            color.R = (float)(sL * 2.55);
            color.G = color.R;
            color.B = color.R;
        }
        else
        {
            sH = (sH + 3600000) % 360;
            double[] cent =
            [
                +0.1, -0.06, -0.3, // 0, 30, 60
                -0.19, -0.15, -0.24, // 90, 120, 150
                -0.32, -0.09, +0.18, // 180, 210, 240
                +0.05, -0.12, -0.02, // 270, 300, 330
                +0.1, -0.06
            ]; // 最后两位与前两位一致，加是变亮，减是变暗
            var center = sH / 30.0;
            var intCenter = (int)Math.Floor(center); // 亮度片区编号
            center = 50 - (
                (1 - center + intCenter) * cent[intCenter] + (center - intCenter) * cent[intCenter + 1]
            ) * sS;

            sL = sL < center ? sL / center : 1 + (sL - center) / (100 - center);
            sL *= 50;
            color = FromHsl(sH, sS, sL);
        }

        color.A = 255;
        return color;
    }
}
