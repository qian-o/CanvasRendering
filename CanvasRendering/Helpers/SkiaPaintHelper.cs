using SkiaSharp;

namespace CanvasRendering.Helpers;

public static class SkiaPaintHelper
{
    private static readonly Dictionary<string, SKTypeface> _typeface = new();
    private static readonly Dictionary<string, SKPaint> _fillPaints = new();
    private static readonly Dictionary<string, SKPaint> _strokePaints = new();
    private static readonly Dictionary<string, SKPaint> _textPaints = new();

    public static SKPaint GetFillPaint(SKColor color, SKBlendMode mode)
    {
        string key = $"{color}-{mode}";

        if (!_fillPaints.TryGetValue(key, out SKPaint paint))
        {
            paint = GetDefaultPaint();
            paint.Style = SKPaintStyle.Fill;
            paint.Color = color;
            paint.BlendMode = mode;

            _fillPaints.Add(key, paint);
        }

        return paint;
    }

    public static SKPaint GetStrokePaint(SKColor color, float strokeWidth)
    {
        string key = $"{color}-{strokeWidth}";

        if (!_strokePaints.TryGetValue(key, out SKPaint paint))
        {
            paint = GetDefaultPaint();
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = color;
            paint.StrokeWidth = strokeWidth;

            _strokePaints.Add(key, paint);
        }

        return paint;
    }

    public static SKPaint GetTextPaint(SKColor color, float textSize, string fontPath)
    {
        string key = $"{color}-{textSize}-{fontPath}";

        if (!_typeface.TryGetValue(fontPath, out SKTypeface typeface))
        {
            typeface = SKTypeface.FromStream(FileManager.LoadFile(fontPath));

            _typeface.Add(fontPath, typeface);
        }

        if (!_textPaints.TryGetValue(key, out SKPaint paint))
        {
            paint = GetDefaultPaint();
            paint.Style = SKPaintStyle.Fill;
            paint.Color = color;
            paint.TextSize = textSize;
            paint.Typeface = typeface;

            _textPaints.Add(key, paint);
        }

        return paint;
    }

    private static SKPaint GetDefaultPaint()
    {
        return new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High
        };
    }
}
