using CanvasRendering.Contracts;
using Silk.NET.Maths;
using System.Drawing;

namespace CanvasRendering.Helpers;

public class SkiaCanvas : ICanvas
{
    public void Begin()
    {
        throw new NotImplementedException();
    }

    public void End()
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        throw new NotImplementedException();
    }

    public void DrawCircle(PointF origin, float radius, Color color)
    {
        throw new NotImplementedException();
    }

    public void DrawLine(PointF start, PointF end, float width, Color color)
    {
        throw new NotImplementedException();
    }

    public void DrawString(Point point, string text, uint size, Color color, string fontPath)
    {
        throw new NotImplementedException();
    }

    public void DrawCanvas(ICanvas canvas, Rectangle<int> rectangle, bool clipToBounds)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
