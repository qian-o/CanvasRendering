using Silk.NET.Maths;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace CanvasRendering.Helpers;

public unsafe class Canvas : IDisposable
{
    private readonly GL _gl;
    private readonly ShaderHelper _shaderHelper;
    private readonly Rectangle<int> _rectangle;
    private readonly float _scale;
    private readonly Vector2D<uint> _actualSize;

    public Framebuffer Framebuffer { get; }

    public uint VertexBuffer { get; }

    public uint TexCoordBuffer { get; }

    public Canvas(GL gl, ShaderHelper shaderHelper, Rectangle<int> rectangle)
    {
        _gl = gl;
        _shaderHelper = shaderHelper;
        _rectangle = rectangle;

        _gl.GetInteger(GLEnum.MaxTextureSize, out int maxTextureSize);

        if (_rectangle.Size.X * _rectangle.Size.Y > maxTextureSize)
        {
            _scale = Convert.ToSingle(Math.Sqrt((double)maxTextureSize / (_rectangle.Size.X * _rectangle.Size.Y)));

            _actualSize = new Vector2D<uint>(Convert.ToUInt32(_rectangle.Size.X * _scale), Convert.ToUInt32(_rectangle.Size.Y * _scale));
        }
        else
        {
            _scale = 1;

            _actualSize = new Vector2D<uint>((uint)_rectangle.Size.X, (uint)_rectangle.Size.Y);
        }

        Framebuffer = new Framebuffer(gl, _actualSize);

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

    public void DrawRectangle(RectangleF rectangle, Color color)
    {
        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, Framebuffer.Fbo);

        _gl.Viewport(0, 0, _actualSize.X, _actualSize.Y);

        ShaderProgram shaderProgram = new(_gl);
        shaderProgram.Attach(_shaderHelper.GetShader("defaultVertex.vert"), _shaderHelper.GetShader("solidColor.frag"));

        uint positionAttrib = (uint)_gl.GetAttribLocation(shaderProgram.Id, "position");

        float[] vertices = new float[] {
            rectangle.Left, rectangle.Top, 0.0f,
            rectangle.Left, rectangle.Bottom, 0.0f,
            rectangle.Right, rectangle.Top, 0.0f,
            rectangle.Right, rectangle.Bottom, 0.0f
        };

        uint vbo = _gl.GenBuffer();

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.BufferData<float>(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.EnableVertexAttribArray(positionAttrib);

        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        _gl.VertexAttribPointer(positionAttrib, 3, GLEnum.Float, false, 0, null);

        _gl.UseProgram(shaderProgram.Id);

        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0.0f, _actualSize.X, _actualSize.Y, 0.0f, -1.0f, 1.0f);

        _gl.UniformMatrix4(_gl.GetUniformLocation(shaderProgram.Id, "projection"), 1, false, (float*)&projection);

        _gl.Uniform4(_gl.GetUniformLocation(shaderProgram.Id, "solidColor"), ColorToVector4(color));

        _gl.DrawArrays(GLEnum.TriangleStrip, 0, 4);

        _gl.UseProgram(0);

        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);

        _gl.DisableVertexAttribArray(positionAttrib);

        _gl.DeleteBuffer(vbo);

        _gl.DeleteProgram(shaderProgram.Id);

        _gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public void Dispose()
    {
        Framebuffer.Dispose();

        _gl.DeleteBuffer(VertexBuffer);
        _gl.DeleteBuffer(TexCoordBuffer);

        GC.SuppressFinalize(this);
    }

    private static Vector4 ColorToVector4(Color color)
    {
        return new Vector4(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
}
