using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public static class ColorHelper
{
    public static Vector4 ToVector4(this Color color)
    {
        return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    public static Color ToColor(this Vector4 hsv)
    {
        float num = hsv.X * 360f;
        float y = hsv.Y;
        float z = hsv.Z;
        float num2 = z * y;
        float num3 = num / 60f;
        float num4 = num2 * (1f - Math.Abs(num3 % 2f - 1f));
        float num5;
        float num6;
        float num7;
        if (num3 >= 0f && num3 < 1f)
        {
            num5 = num2;
            num6 = num4;
            num7 = 0f;
        }
        else if (num3 >= 1f && num3 < 2f)
        {
            num5 = num4;
            num6 = num2;
            num7 = 0f;
        }
        else if (num3 >= 2f && num3 < 3f)
        {
            num5 = 0f;
            num6 = num2;
            num7 = num4;
        }
        else if (num3 >= 3f && num3 < 4f)
        {
            num5 = 0f;
            num6 = num4;
            num7 = num2;
        }
        else if (num3 >= 4f && num3 < 5f)
        {
            num5 = num4;
            num6 = 0f;
            num7 = num2;
        }
        else if (num3 >= 5f && num3 < 6f)
        {
            num5 = num2;
            num6 = 0f;
            num7 = num4;
        }
        else
        {
            num5 = 0f;
            num6 = 0f;
            num7 = 0f;
        }

        float num8 = z - num2;

        return Color.FromArgb(ToByte(num7 + num8), ToByte(hsv.W), ToByte(num5 + num8), ToByte(num6 + num8));
    }

    private static byte ToByte(float component)
    {
        return ToByte((int)(component * 255f));
    }

    public static byte ToByte(int value)
    {
        return (byte)((value >= 0) ? ((value > 255) ? 255u : ((uint)value)) : 0u);
    }
}
