using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering;

public unsafe class Canvas
{
    private readonly GL _gl;
    private readonly Rectangle<int> _rectangle;

    private readonly Shader vs;
    private readonly Shader fs;
    private readonly ShaderProgram program;
    private float scale;
    private Vector2D<int> actualSize;

    public uint Texture { get; private set; }

    public uint Fbo { get; private set; }

    public Canvas(GL gl, Rectangle<int> rectangle)
    {
        _gl = gl;
        _rectangle = rectangle;

        // 创建顶点着色器
        vs = new(gl);
        vs.LoadShader(GLEnum.VertexShader, "Shaders/canvas.vert");

        // 创建片段着色器
        fs = new(gl);
        fs.LoadShader(GLEnum.FragmentShader, "Shaders/solidColor.frag");

        program = new ShaderProgram(gl);
        program.AttachShader(vs, fs);

        LoadFrame();
    }

    public void Clear()
    {
        BindFrame();

        _gl.ClearColor(Color.SteelBlue);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        UnbindingFrame();
    }

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        float[] vertices = new float[] {
            rectangle.X * scale, rectangle.Y * scale,
            rectangle.X * scale, rectangle.Bottom * scale,
            rectangle.Right * scale,  rectangle.Y * scale,
            rectangle.Right * scale, rectangle.Bottom * scale
        };

        fixed (void* pointer = vertices)
        {
            BindFrame();

            _gl.Uniform4(program.GetUniformLocation("solidColor"), ColorToVector4(color));

            _gl.VertexAttribPointer(0, 2, GLEnum.Float, false, 0, pointer);

            _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

            UnbindingFrame();
        }
    }

    private void LoadFrame()
    {
        _gl.GetInteger(GLEnum.ImplementationColorReadFormat, out int format);
        _gl.GetInteger(GLEnum.ImplementationColorReadType, out int type);
        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            scale = Convert.ToSingle(Math.Sqrt((double)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y)));

            actualSize = new Vector2D<int>(Convert.ToInt32(_rectangle.Size.X * scale), Convert.ToInt32(_rectangle.Size.Y * scale));
        }
        else
        {
            actualSize = new Vector2D<int>(_rectangle.Size.X, _rectangle.Size.Y);
        }

        Texture = _gl.GenTexture();
        _gl.BindTexture(TextureTarget.Texture2D, Texture);
        _gl.TexImage2D(GLEnum.Texture2D, 0, (int)InternalFormat.Rgba, (uint)actualSize.X, (uint)actualSize.Y, 0, (PixelFormat)format, (PixelType)type, IntPtr.Zero);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.BindTexture(TextureTarget.Texture2D, 0);

        Fbo = _gl.GenFramebuffer();
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);
        _gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, Texture, 0);
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void BindFrame()
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Fbo);

        _gl.Viewport(0, 0, (uint)actualSize.X, (uint)actualSize.Y);

        program.Use();

        _gl.EnableVertexAttribArray(0);
        _gl.EnableVertexAttribArray(1);
    }

    private void UnbindingFrame()
    {
        _gl.DisableVertexAttribArray(0);
        _gl.DisableVertexAttribArray(1);

        _gl.UseProgram(0);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}