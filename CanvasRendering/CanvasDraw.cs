using CanvasRendering.Controls;
using CanvasRendering.Helpers;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;

namespace CanvasRendering;

public unsafe static class CanvasDraw
{
    private static readonly List<double> fpsSample = new();

    private static GL _gl;
    private static ShaderHelper _shaderHelper;
    private static ShaderProgram _textureProgram;
    private static TestControl1 _c1;
    private static TestControl1 _c2;
    private static TestControl1 _c3;
    private static TestControl1 _c4;
    private static UniformGrid _uniformGrid;
    private static FpsControl fpsControl;
    private static int _angle;
    private static float _radians;
    private static float _radiansX;
    private static float _radiansY;
    private static int _translation;
    private static float _scale = 1.0f;
    private static bool firstMove = true;
    private static Vector2D<float> lastPos;

    public static int Width { get; set; }

    public static int Height { get; set; }

    public static Camera Camera { get; } = new Camera();

    public static string FontPath { get; set; }

    public static IMouse Mouse { get; set; }

    public static IKeyboard Keyboard { get; set; }

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _shaderHelper = new ShaderHelper(_gl);

        _textureProgram = new ShaderProgram(_gl);
        _textureProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        Width = width;
        Height = height;
        Camera.Width = Width;
        Camera.Height = Height;

        _c1 = new TestControl1(_gl)
        {
            Text = "Rotation"
        };

        _c2 = new TestControl1(_gl)
        {
            Text = "Scale"
        };

        _c3 = new TestControl1(_gl)
        {
            Text = "Skew"
        };

        _c4 = new TestControl1(_gl)
        {
            Text = "Translation"
        };
        _uniformGrid = new UniformGrid()
        {
            Rows = 100,
            Columns = 100
        };

        for (int i = 0; i < 10000; i++)
        {
            _uniformGrid.Child.Add(new TestControl1(_gl) { Text = (i + 1).ToString() });
        }

        fpsControl = new FpsControl(_gl);
    }

    public static void FramebufferResize(Vector2D<int> obj)
    {
        Width = obj.X;
        Height = obj.Y;

        _gl.Viewport(0, 0, (uint)Width, (uint)Height);

        Camera.Width = Width;
        Camera.Height = Height;
    }

    public static void Render(double obj)
    {
        _gl.ClearColor(Color.Black);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _c1.StartRender();
        _c1.DrawOnWindow(_textureProgram);

        _c2.StartRender();
        _c2.DrawOnWindow(_textureProgram);

        _c3.StartRender();
        _c3.DrawOnWindow(_textureProgram);

        _c4.StartRender();
        _c4.DrawOnWindow(_textureProgram);

        _uniformGrid.Width = Width;
        _uniformGrid.Height = Height;
        _uniformGrid.Render();

        foreach (var item in _uniformGrid.Child)
        {
            item.StartRender();
        }

        foreach (var item in _uniformGrid.Child)
        {
            item.DrawOnWindow(_textureProgram);
        }

        fpsControl.StartRender();
        fpsControl.DrawOnWindow(_textureProgram);

        fpsSample.Add(1.0d / obj);
    }

    public static void Update(double obj)
    {
        int w = Width / 2;
        int h = Height / 2;

        _c1.Left = w * 0 + w / 4;
        _c2.Left = w * 1 + w / 4;
        _c3.Left = w * 0 + w / 4;
        _c4.Left = w * 1 + w / 4;

        _c1.Top = h * 0 + h / 4;
        _c2.Top = h * 0 + h / 4;
        _c3.Top = h * 1 + h / 4;
        _c4.Top = h * 1 + h / 4;

        _c1.Width = Convert.ToUInt32(w / 2);
        _c2.Width = Convert.ToUInt32(w / 2);
        _c3.Width = Convert.ToUInt32(w / 2);
        _c4.Width = Convert.ToUInt32(w / 2);

        _c1.Height = Convert.ToUInt32(h / 2);
        _c2.Height = Convert.ToUInt32(h / 2);
        _c3.Height = Convert.ToUInt32(h / 2);
        _c4.Height = Convert.ToUInt32(h / 2);

        if (Mouse.IsButtonPressed(MouseButton.Left))
        {
            _angle++;

            _radians = _angle * MathF.PI / 180;
            _radiansX = _angle * MathF.PI / 180;
            _radiansY = _angle * MathF.PI / 180;

            _scale += 0.01f;

            _translation++;
        }
        else
        {
            if (_angle > 0)
            {
                _angle--;

                _radians = _angle * MathF.PI / 180;
                _radiansX = _angle * MathF.PI / 180;
                _radiansY = _angle * MathF.PI / 180;
            }

            if (_scale > 1.0f)
            {
                _scale -= 0.01f;
            }

            if (_translation > 0)
            {
                _translation--;
            }
        }

        if (Mouse.IsButtonPressed(MouseButton.Right))
        {
            Vector2D<float> vector = new(Mouse.Position.X, Mouse.Position.Y);

            if (firstMove)
            {
                lastPos = vector;

                firstMove = false;
            }
            else
            {
                float deltaX = vector.X - lastPos.X;
                float deltaY = vector.Y - lastPos.Y;

                Camera.Yaw += deltaX * 0.2f;
                Camera.Pitch += -deltaY * 0.2f;

                lastPos = vector;
            }
        }
        else
        {
            firstMove = true;
        }

        if (Keyboard.IsKeyPressed(Key.W))
        {
            Camera.Position += Camera.Front * 1.5f * (float)obj;
        }

        if (Keyboard.IsKeyPressed(Key.A))
        {
            Camera.Position -= Camera.Right * 1.5f * (float)obj;
        }

        if (Keyboard.IsKeyPressed(Key.S))
        {
            Camera.Position -= Camera.Front * 1.5f * (float)obj;
        }

        if (Keyboard.IsKeyPressed(Key.D))
        {
            Camera.Position += Camera.Right * 1.5f * (float)obj;
        }

        if (Keyboard.IsKeyPressed(Key.Q))
        {
            Camera.Position -= Camera.Up * 1.5f * (float)obj;
        }

        if (Keyboard.IsKeyPressed(Key.E))
        {
            Camera.Position += Camera.Up * 1.5f * (float)obj;
        }

        _c1.Transform = Matrix3X2.CreateRotation(_radians, new Vector2D<float>(_c1.Width / 2.0f, _c1.Height / 2.0f));
        _c2.Transform = Matrix3X2.CreateScale(_scale, new Vector2D<float>(_c2.Width / 2.0f, _c2.Height / 2.0f));
        _c3.Transform = Matrix3X2.CreateSkew(_radiansX, _radiansY, new Vector2D<float>(_c3.Width / 2.0f, _c3.Height / 2.0f));
        _c4.Transform = Matrix3X2.CreateTranslation(new Vector2D<float>(_translation, _translation));

        if (fpsSample.Count == 100)
        {
            fpsControl.Fps = Convert.ToInt32(fpsSample.Average());

            fpsSample.Clear();
        }
    }
}