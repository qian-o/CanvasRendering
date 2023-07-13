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

        _shaderHelper = new ShaderHelper(_gl);

        _textureProgram = new ShaderProgram(_gl);
        _textureProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        _width = width;
        _height = height;
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

        _gl.ClearColor(Color.White);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        Matrix4x4 orthographic = Matrix4x4.CreateOrthographicOffCenter(0.0f, _width, _height, 0.0f, 0.0f, 1.0f);

        {
            Vector2 centerPoint = new(_c1.Left + (_c1.Width / 2.0f), _c1.Top + (_c1.Height / 2.0f));
            centerPoint = Vector2.Transform(centerPoint, orthographic);

            Matrix4x4 matrix = Matrix4x4.CreateTranslation(new Vector3(-centerPoint.X, -centerPoint.Y, 0.0f));
            matrix *= Matrix4x4.CreateRotationX((float)(_angle * Math.PI / 180.0), new Vector3(0.0f, 0.0f, 1.0f));
            _c1.RenderTransform = matrix;

            matrix = Matrix4x4.CreateTranslation(new Vector3(centerPoint.X, centerPoint.Y, 0.0f));
            _c1.LayoutTransform = matrix;
        }

        {
            Vector2 centerPoint = new(_c2.Left + (_c2.Width / 2.0f), _c2.Top + (_c2.Height / 2.0f));
            centerPoint = Vector2.Transform(centerPoint, orthographic);

            Matrix4x4 matrix = Matrix4x4.CreateTranslation(new Vector3(-centerPoint.X, -centerPoint.Y, 0.0f));
            matrix *= Matrix4x4.CreateRotationY((float)(_angle * Math.PI / 180.0), new Vector3(0.0f, 0.0f, 1.0f));
            _c2.RenderTransform = matrix;

            matrix = Matrix4x4.CreateTranslation(new Vector3(centerPoint.X, centerPoint.Y, 0.0f));
            _c2.LayoutTransform = matrix;
        }

        {
            Vector3 centerPoint = new(_c3.Left + (_c3.Width / 2.0f), _c3.Top + (_c3.Height / 2.0f), 1.0f);
            centerPoint = Vector3.Transform(centerPoint, orthographic);

            Matrix4x4 matrix = Matrix4x4.CreateTranslation(new Vector3(-centerPoint.X, -centerPoint.Y, 0.0f));
            matrix *= Matrix4x4.CreateRotationZ((float)(_angle * Math.PI / 180.0));
            _c3.RenderTransform = matrix;

            matrix = Matrix4x4.CreateTranslation(new Vector3(centerPoint.X, centerPoint.Y, 0.0f));
            _c3.LayoutTransform = matrix;
        }

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
