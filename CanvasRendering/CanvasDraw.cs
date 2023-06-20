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
    private static uint _positionAttrib;
    private static uint _texCoordAttrib;
    private static int _width, _height;
    private static Canvas _canvas;
    private static Stopwatch _stopwatch;

    public static void Load(GL gl, int width, int height)
    {
        _gl = gl;

        _gl.ClearColor(Color.White);

        _shaderHelper = new ShaderHelper(_gl);

        _shaderProgram = new ShaderProgram(_gl);
        _shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("texture.frag"));

        _positionAttrib = (uint)_shaderProgram.GetAttribLocation("position");
        _texCoordAttrib = (uint)_shaderProgram.GetAttribLocation("texCoord");

        _width = width;
        _height = height;

        _canvas = new(_gl, _shaderHelper, new Rectangle<int>(0, 0, width, height));
        _stopwatch = Stopwatch.StartNew();
    }

    public static void Resize(Vector2D<int> obj)
    {
        _width = obj.X;
        _height = obj.Y;

        _gl.Viewport(0, 0, (uint)_width, (uint)_height);

        _canvas.Resize(new Rectangle<int>(0, 0, _width, _height));
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
        }
        _canvas.End();

        DrawCanvas(_canvas);
    }

    private static void DrawCanvas(Canvas canvas)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        _gl.EnableVertexAttribArray(_positionAttrib);
        _gl.EnableVertexAttribArray(_texCoordAttrib);

        _shaderProgram.Enable();

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, _width, _height, 0.0f, -1.0f, 1.0f);

        _gl.UniformMatrix4(_shaderProgram.GetUniformLocation("projection"), 1, false, (float*)&projection);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.VertexBuffer);
        _gl.VertexAttribPointer(_positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.BindBuffer(GLEnum.ArrayBuffer, canvas.TexCoordBuffer);
        _gl.VertexAttribPointer(_texCoordAttrib, 2, GLEnum.Float, false, 0, null);

        _gl.ActiveTexture(GLEnum.Texture0);
        _gl.BindTexture(GLEnum.Texture2D, canvas.Framebuffer.DrawTexture);
        _gl.Uniform1(_shaderProgram.GetUniformLocation("tex"), 0);

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        _gl.BindTexture(GLEnum.Texture2D, 0);

        _shaderProgram.Disable();

        _gl.DisableVertexAttribArray(_positionAttrib);
        _gl.DisableVertexAttribArray(_texCoordAttrib);
    }
}
