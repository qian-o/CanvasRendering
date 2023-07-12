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
    private static TestControl1 _c2;
    private static TestControl1 _c3;
    private static int _angle;

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
            Left = 0,
            Top = 0,
            Width = 400,
            Height = 400
        };

        _c2 = new TestControl1(_gl)
        {
            Left = 600,
            Top = 100,
            Width = 400,
            Height = 400
        };

        _c3 = new TestControl1(_gl)
        {
            Left = 100,
            Top = 600,
            Width = 400,
            Height = 400
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

        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        // _c1.LayoutTransform = Matrix4x4.CreateRotationY((float)(_angle * Math.PI / 180.0), new Vector3(200, -30, 0.0f));
        // _c2.LayoutTransform = Matrix4x4.CreateTranslation(new Vector3(0.0f, 0, -15.0f));

        _c1.StartRender();
        _c1.DrawOnWindow(_width, _height, _textureProgram);

        _c2.StartRender();
        _c2.DrawOnWindow(_width, _height, _textureProgram);

        _c3.StartRender();
        _c3.DrawOnWindow(_width, _height, _textureProgram);

        _angle += 1;
        if (_angle == 360)
        {
            _angle = 0;
        }

        if (fpsSample.Count == 60)
        {
            Fps = Convert.ToInt32(fpsSample.Average());

            fpsSample.Clear();
        }

        fpsSample.Add(Convert.ToInt32(1 / obj));
    }
}
