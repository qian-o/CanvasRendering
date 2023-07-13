using CanvasRendering.Contracts.Controls;
using Silk.NET.OpenGLES;
using System.Drawing;

namespace CanvasRendering.Controls;

public class TestControl1 : BaseControl
{
    public string Text { get; set; }

    public Color Fill { get; set; } = Color.FromArgb(104, 199, 232);

    public TestControl1(GL gl) : base(gl)
    {
    }

    protected override void OnRender()
    {
        Canvas.DrawFill(Fill);
        Canvas.DrawRectangle(new RectangleF(100, 100, Width, Height), Color.FromArgb(255, 220, 1));
        Canvas.DrawString(new Point(20, 40), Text, 24, Color.Black, CanvasDraw.FontPath);
    }
}
