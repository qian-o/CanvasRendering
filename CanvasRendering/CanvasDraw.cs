using CanvasRendering.Controls;
using CanvasRendering.Helpers;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;

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
    private static FpsControl fpsControl;
    private static int _angle;
    private static bool isPointerDown;

    public static int Width { get; set; }

    public static int Height { get; set; }

    public static string FontPath { get; set; }

    public static int Fps { get; set; }

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
            Left = 100,
            Top = 100,
            Width = 200,
            Height = 400,
            Text = "X 轴 旋转"
        };

        _c2 = new TestControl1(_gl)
        {
            Left = Width - 400,
            Top = 100,
            Width = 400,
            Height = 200,
            Text = "Y 轴 旋转"
        };

        _c3 = new TestControl1(_gl)
        {
            Left = Width - 400,
            Top = Height - 200,
            Width = 400,
            Height = 200,
            Text = "Z 轴 旋转"
        };

        fpsControl = new FpsControl(_gl);
    }

    public static void Resize(Vector2D<int> obj)
    {
        Width = obj.X;
        Height = obj.Y;

        _gl.Viewport(0, 0, (uint)Width, (uint)Height);
    }

    public static void Render(double obj)
    {
        _ = obj;

        _gl.ClearColor(Color.White);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _c1.Transform = Matrix4X4.CreateRotationX(_angle * MathF.PI / 180, new Vector3D<float>(Width / 2.0f, Height / 2.0f, 0.0f));
        _c1.TransformOrigin = new(0.0f, 0.0f, 0.0f);

        _c2.Transform = Matrix4X4.CreateRotationY(_angle * MathF.PI / 180, new Vector3D<float>(Width / 2.0f, Height / 2.0f, 0.0f));
        _c2.TransformOrigin = new(0.0f, 0.0f, 0.0f);

        _c3.Transform = Matrix4X4.CreateRotationZ(_angle * MathF.PI / 180, new Vector3D<float>(Width / 2.0f, Height / 2.0f, 0.0f));
        _c3.TransformOrigin = new(0.0f, 0.0f, 0.0f);

        _c1.StartRender();
        _c1.DrawOnWindow(_textureProgram);

        _c2.StartRender();
        _c2.DrawOnWindow(_textureProgram);

        _c3.StartRender();
        _c3.DrawOnWindow(_textureProgram);

        fpsControl.StartRender();
        fpsControl.DrawOnWindow(_textureProgram);
    }

    public static void Update(double obj)
    {
        _c2.Left = Width - 400;
        _c3.Left = Width - 400;
        _c3.Top = Height - 200;
        fpsControl.Fps = Fps;

        if (isPointerDown)
        {
            _angle += 1;
            if (_angle == 360)
            {
                _angle = 0;
            }
        }

        if (fpsSample.Count == 60)
        {
            Fps = Convert.ToInt32(fpsSample.Average());

            fpsSample.Clear();
        }

        fpsSample.Add(Convert.ToInt32(1 / obj));
    }

    public static void PointerDown()
    {
        isPointerDown = true;
    }

    public static void PointerUp()
    {
        isPointerDown = false;
    }
}
