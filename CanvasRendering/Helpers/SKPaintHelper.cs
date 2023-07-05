using SkiaSharp;

namespace CanvasRendering.Helpers;

public static class SKPaintHelper
{
    private static SKPaint paint;

    public static SKPaint GetDefaultPaint()
    {
        paint?.Dispose();
        paint = new SKPaint
        {
            IsAntialias = true,
            IsDither = true,
            FilterQuality = SKFilterQuality.High
        };

        return paint;
    }
}
