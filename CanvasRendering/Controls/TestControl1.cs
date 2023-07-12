using CanvasRendering.Contracts.Controls;
using Silk.NET.OpenGLES;
using System.Drawing;

namespace CanvasRendering.Controls;

public class TestControl1 : BaseControl
{
    public string Text { get; set; }

    public TestControl1(GL gl) : base(gl)
    {
    }

    protected override void OnRender()
    {
        Canvas.DrawRectangle(new RectangleF(0, 0, Width, Height), Color.FromArgb(104, 199, 232));
        Canvas.DrawRectangle(new RectangleF(100, 100, Width, Height), Color.FromArgb(255, 220, 1));
        Canvas.DrawString(new Point(20, 40), Text, 24, Color.Black, CanvasDraw.FontPath);
    }
}
