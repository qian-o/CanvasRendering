using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe class Canvas
{
    private readonly GL _gl;
    private readonly Rectangle<int> _rectangle;
    private readonly Shader _vs;
    private readonly Shader _solidColor;
    private readonly ShaderProgram _program;
    private readonly float _scale;
    private readonly Vector2D<int> _actualSize;

    public uint Texture { get; }

    public uint Fbo { get; }

    public uint VertexBuffer { get; }

    public uint TexCoordBuffer { get; }

    public Canvas(GL gl, Rectangle<int> rectangle)
    {
        _gl = gl;
        _rectangle = rectangle;

        _vs = new(gl);
        _vs.LoadShader(GLEnum.VertexShader, "Shaders/canvas.vert");

        _solidColor = new(gl);
        _solidColor.LoadShader(GLEnum.FragmentShader, "Shaders/solidColor.frag");

        _program = new ShaderProgram(gl);

        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);
        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            _scale = Convert.ToSingle(Math.Sqrt((double)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y)));

            _actualSize = new Vector2D<int>(Convert.ToInt32(_rectangle.Size.X * _scale), Convert.ToInt32(_rectangle.Size.Y * _scale));
        }
        else
        {
            _actualSize = new Vector2D<int>(_rectangle.Size.X, _rectangle.Size.Y);
        }

        Texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Texture);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, (uint)_actualSize.X, (uint)_actualSize.Y, 0, (PixelFormat)format, (PixelType)type, IntPtr.Zero);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        Fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture, 0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        float[] vertices = new float[] {
            _rectangle.Origin.X, _rectangle.Origin.Y,
            _rectangle.Origin.X, _rectangle.Max.Y,
            _rectangle.Max.X, _rectangle.Origin.Y,
            _rectangle.Max.X, _rectangle.Max.Y
        };

        VertexBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, VertexBuffer);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        float[] texCoords = new float[] {
            0, 0,
            0, _rectangle.Size.Y,
            _rectangle.Size.X, 0,
            _rectangle.Size.X, _rectangle.Size.Y
        };

        TexCoordBuffer = _gl.GenBuffer();
        _gl.BindBuffer(GLEnum.ArrayBuffer, TexCoordBuffer);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(texCoords.Length * sizeof(float)), texCoords, BufferUsageARB.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
    }

    public void Clear()
    {
        Draw(() =>
        {
            _gl.ClearColor(Color.Black);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        });
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        Draw(() =>
        {
            float[] vertices = new float[] {
                rectangle.X * _scale, rectangle.Y * _scale,
                rectangle.X * _scale, rectangle.Bottom * _scale,
                rectangle.Right * _scale,  rectangle.Y * _scale,
                rectangle.Right * _scale, rectangle.Bottom * _scale
            };

            _program.AttachShader(_vs, _solidColor);

            _program.Use();

            _gl.Uniform4(_program.GetUniformLocation("solidColor"), ColorToVector4(color));

            _gl.EnableVertexAttribArray(0);

            uint vbo = _gl.GenBuffer();
            _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
            _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 0, null);

            _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

            _gl.DisableVertexAttribArray(0);

            _gl.DeleteBuffer(vbo);
        });
    }

    private void Draw(Action action)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        _gl.Viewport(0, 0, (uint)_actualSize.X, (uint)_actualSize.Y);

        action?.Invoke();

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}