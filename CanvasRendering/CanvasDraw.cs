using CanvasRendering.Controls;
using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe static class CanvasDraw
{
    private static readonly List<int> fpsSample = new();

    private static GL _gl;
    private static ShaderHelper _shaderHelper;
    private static ShaderProgram _textureProgram;
    private static int _width, _height;
    private static TestControl1 _c1;

    public static string FontPath { get; set; }

    public static int Fps { get; set; }

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _gl.ClearColor(Color.White);

        _shaderHelper = new ShaderHelper(_gl);

        _textureProgram = new ShaderProgram(_gl);
        _textureProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        _width = width;
        _height = height;
        _c1 = new TestControl1(_gl)
        {
            Left = 100,
            Top = 100,
            Width = 400,
            Height = 400,
            LayoutTransform = Matrix4x4.CreateScale(new Vector3(0.5f, 1, 1)),
            RenderTransform = Matrix4x4.CreateScale(new Vector3(0.5f, 1, 1))
        };
    }

    public static void Resize(Vector2D<int> obj)
    {
        _width = obj.X;
        _height = obj.Y;

        _gl.Viewport(0, 0, (uint)_width, (uint)_height);
    }

    public static void Render(double obj)
    {
        _ = obj;

        _c1.StartRender();
        _c1.DrawOnWindow(_width, _height, _textureProgram);

        if (fpsSample.Count == 60)
        {
            Fps = Convert.ToInt32(fpsSample.Average());

            fpsSample.Clear();
        }

        fpsSample.Add(Convert.ToInt32(1 / obj));
    }
}
