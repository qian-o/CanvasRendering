using CanvasRendering.Contracts.Controls;
using Silk.NET.OpenGLES;
using System.Drawing;

namespace CanvasRendering.Controls;

public class FpsControl : BaseControl
{
    private int fps;

    public int Fps { get => fps; set { fps = value; UpdateRender(); } }

    public FpsControl(GL gl) : base(gl)
    {
        Width = 100;
        Height = 100;
    }

    protected override void OnRender()
    {
        Canvas.DrawFill(Color.Transparent);

        Canvas.DrawString(new Point(25, 75), Fps.ToString(), 50, Color.Green, CanvasDraw.FontPath);
    }
}
