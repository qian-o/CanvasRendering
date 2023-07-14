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
    private static TestControl1 _c1;
    private static TestControl1 _c2;
    private static TestControl1 _c3;
    private static int _angle;

    public static int Width { get; set; }

    public static int Height { get; set; }

    public static string FontPath { get; set; }

    public static int Fps { get; set; }

    public static Matrix4x4 Orthographic { get; set; }

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _shaderHelper = new ShaderHelper(_gl);

        _textureProgram = new ShaderProgram(_gl);
        _textureProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        Width = width;
        Height = height;
        _c1 = new TestControl1(_gl)
        {
            Left = 0,
            Top = 0,
            Width = 200,
            Height = 400,
            Text = "X 轴 旋转"
        };

        _c2 = new TestControl1(_gl)
        {
            Left = 100,
            Top = 100,
            Width = 400,
            Height = 200,
            Text = "Y 轴 旋转",
            Fill = Color.Transparent
        };

        _c3 = new TestControl1(_gl)
        {
            Left = 100,
            Top = 600,
            Width = 400,
            Height = 200,
            Text = "Z 轴 旋转"
        };

        Orthographic = Matrix4x4.CreateOrthographicOffCenter(0.0f, Width, Height, 0.0f, 1.0f, -1.0f);
    }

    public static void Resize(Vector2D<int> obj)
    {
        Width = obj.X;
        Height = obj.Y;

        _gl.Viewport(0, 0, (uint)Width, (uint)Height);

        Orthographic = Matrix4x4.CreateOrthographicOffCenter(0.0f, Width, Height, 0.0f, 1.0f, -1.0f);
    }

    public static void Render(double obj)
    {
        _ = obj;

        _gl.ClearColor(Color.White);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _c1.Transform = Matrix4x4.CreateRotationX(_angle * MathF.PI / 180);
        _c1.TransformOrigin = new Vector3(0.5f, 0.5f, 0.0f);

        _c2.Transform = Matrix4x4.CreateRotationY(_angle * MathF.PI / 180);
        _c2.TransformOrigin = new Vector3(0.5f, 0.5f, 0.0f);

        _c3.Transform = Matrix4x4.CreateRotationZ(_angle * MathF.PI / 180);
        _c3.TransformOrigin = new Vector3(0.5f, 0.5f, 0.0f);

        _c1.StartRender();
        _c1.DrawOnWindow(_textureProgram);

        _c2.StartRender();
        _c2.DrawOnWindow(_textureProgram);

        _c3.StartRender();
        _c3.DrawOnWindow(_textureProgram);

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
