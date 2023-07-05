using CanvasRendering.Contracts;
using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe static class CanvasDraw
{
    private static GL _gl;
    private static ShaderHelper _shaderHelper;
    private static ShaderProgram _shaderProgram;
    private static int _width, _height;
    private static ICanvas _canvas;
    private static Stopwatch _stopwatch;
    private static readonly List<int> _fpsSample = new();

    public static string FontPath { get; set; }

    public static int Fps { get; set; }

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _gl.ClearColor(Color.White);

        _shaderHelper = new ShaderHelper(_gl);

        _shaderProgram = new ShaderProgram(_gl);
        _shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        _width = width;
        _height = height;

        _canvas = new SkiaCanvas(_gl, new Vector2D<uint>((uint)width, (uint)height));
        _stopwatch = Stopwatch.StartNew();
    }

    public static void Resize(Vector2D<int> obj)
    {
        _width = obj.X;
        _height = obj.Y;

        _gl.Viewport(0, 0, (uint)_width, (uint)_height);

        _canvas?.Dispose();
        _canvas = new SkiaCanvas(_gl, new Vector2D<uint>((uint)_width, (uint)_height));
    }

    public static void Render(double obj)
    {
        _ = obj;

        _canvas.Begin();
        {
            _canvas.Clear();

            float wSum = (float)_width / 20;
            float hSum = (float)_height / 20;

            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    _canvas.DrawLine(new PointF(wSum * i, hSum * j), new PointF(wSum * i, hSum * j + hSum), 1, Color.Black);
                    _canvas.DrawLine(new PointF(wSum * i, hSum * j + hSum), new PointF(wSum * i + wSum, hSum * j + hSum), 1, Color.Black);
                    _canvas.DrawLine(new PointF(wSum * i + wSum, hSum * j + hSum), new PointF(wSum * i + wSum, hSum * j), 1, Color.Black);
                    _canvas.DrawLine(new PointF(wSum * i + wSum, hSum * j), new PointF(wSum * i, hSum * j), 1, Color.Black);

                    float hue = (float)_stopwatch.Elapsed.TotalSeconds * 0.15f % 1;

                    _canvas.DrawRectangle(new RectangleF(wSum * i + wSum / 4, hSum * j + hSum / 4, wSum / 2, hSum / 2), new Vector4(1.0f * hue, 1.0f * 0.75f, 1.0f * 0.75f, 1.0f).ToColor());

                    hue = (float)_stopwatch.Elapsed.TotalSeconds * 0.30f % 1;

                    _canvas.DrawCircle(new PointF(wSum * i + wSum / 2, hSum * j + hSum / 2), 10, new Vector4(1.0f * hue, 1.0f * 0.75f, 1.0f * 0.75f, 1.0f).ToColor());
                }
            }

            SkiaCanvas canvas = new(_gl, new Vector2D<uint>(200, 200));
            canvas.Begin();
            {
                canvas.Clear();

                canvas.DrawRectangle(new RectangleF(0, 0, 200, 200), Color.Blue);

                canvas.DrawCircle(new PointF(100, 100), 100, Color.Green);

                canvas.DrawLine(new PointF(0, 0), new PointF(200, 200), 2, Color.Azure);
            }
            canvas.End();

            _canvas.DrawCanvas(canvas, new Rectangle<int>(100, 100, 150, 120), true);

            canvas.Dispose();

            _canvas.DrawString(new Point(10, 40), "王先生123123ASD ASD ASDF ASD ASDF ", 40, Color.Red, FontPath);
            _canvas.DrawString(new Point(10, 80), "王先生123123ASD ASD ASDF ASD ASDF ", 40, Color.Red, FontPath);
            _canvas.DrawString(new Point(10, 450), "熙", 400, Color.Red, FontPath);

            _canvas.DrawRectangle(new RectangleF(0, 0, 80, 80), Color.Black);
            _canvas.DrawString(new Point(20, 60), Fps.ToString(), 40, Color.Green, FontPath);
        }
        _canvas.End();

        SkiaCanvas.DrawOnWindow(_gl, _shaderProgram, (SkiaCanvas)_canvas);

        if (_fpsSample.Count == 30)
        {
            Fps = Convert.ToInt32(_fpsSample.Average());

            _fpsSample.Clear();
        }

        _fpsSample.Add(Convert.ToInt32(1 / obj));
    }
}
