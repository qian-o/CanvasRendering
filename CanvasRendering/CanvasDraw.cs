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
    private static Canvas _canvas;
    private static Stopwatch _stopwatch;

    public static string FontPath { get; set; }

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _gl.ClearColor(Color.White);

        _shaderHelper = new ShaderHelper(_gl);

        _shaderProgram = new ShaderProgram(_gl);
        _shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        _width = width;
        _height = height;

        _canvas = new(_gl, _shaderHelper, new Vector2D<uint>((uint)width, (uint)height));
        _stopwatch = Stopwatch.StartNew();
    }

    public static void Resize(Vector2D<int> obj)
    {
        _width = obj.X;
        _height = obj.Y;

        _gl.Viewport(0, 0, (uint)_width, (uint)_height);

        _canvas?.Dispose();
        _canvas = new(_gl, _shaderHelper, new Vector2D<uint>((uint)_width, (uint)_height));
    }

    public static void Render(double obj)
    {
        _ = obj;

        _canvas.Begin();
        {
            _canvas.Clear();

            float wSum = (float)_width / 10;
            float hSum = (float)_height / 10;

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
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

            Canvas canvas = new(_gl, _shaderHelper, new Vector2D<uint>(200, 200));
            canvas.Begin();
            {
                canvas.Clear();

                canvas.DrawRectangle(new RectangleF(0, 0, 200, 200), Color.Blue);

                canvas.DrawCircle(new PointF(100, 100), 100, Color.Green);

                canvas.DrawLine(new PointF(0, 0), new PointF(200, 200), 2, Color.Azure);

                canvas.DrawString(new PointF(10, 10), "王先生", 40, Color.Red, FontPath);
            }
            canvas.End();

            _canvas.DrawCanvas(canvas, new Rectangle<int>(100, 100, 150, 120), true);

            canvas.Dispose();
        }
        _canvas.End();

        Canvas.DrawOnWindow(_gl, _shaderProgram, _canvas);
    }
}
