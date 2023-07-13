using System.Drawing;

namespace CanvasRendering.Contracts;

public interface ICanvas : IDisposable
{
    void Begin();

    void End();

    void Clear();

    void DrawFill(Color color);

    void DrawRectangle(RectangleF rectangle, Color color);

    void DrawCircle(PointF origin, float radius, Color color);

    void DrawLine(PointF start, PointF end, float width, Color color);

    void DrawString(Point point, string text, uint size, Color color, string fontPath);
}
